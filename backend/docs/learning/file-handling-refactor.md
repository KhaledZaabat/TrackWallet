# File Handling — Production Refactor

A deep-dive on the file-handling overhaul. Reads top-to-bottom as a study guide:
what was wrong, what each change does, and the .NET / HTTP / OS concepts behind it.

---

## Table of contents

1. [The starting point](#1-the-starting-point)
2. [Defects in the old design](#2-defects-in-the-old-design)
3. [Target architecture](#3-target-architecture)
4. [Step 1 — Add `IFileStorage`](#step-1--add-ifilestorage-pluggable-physical-storage)
5. [Step 2 — Implement `LocalFileStorage`](#step-2--implement-localfilestorage)
6. [Step 3 — Add `ContentHash` to the entity](#step-3--add-contenthash-to-uploadedfile)
7. [Step 4 — Update EF configuration + migration](#step-4--update-ef-configuration--migration)
8. [Step 5 — Streaming DTOs](#step-5--streaming-dtos)
9. [Step 6 — Redesign `IFileService`](#step-6--redesign-ifileservice)
10. [Step 7 — Implement the new `FileService`](#step-7--implement-the-new-fileservice)
11. [Step 8 — `HashingStream`](#step-8--hashingstream)
12. [Step 9 — Update existing handlers](#step-9--update-existing-handlers)
13. [Step 10 — `DeleteFileCommand`](#step-10--delete-command)
14. [Step 11 — Production-grade `FilesController`](#step-11--production-grade-filescontroller)
15. [Concept reference](#concept-reference)

---

## 1. The starting point

The codebase already had:

- `UploadedFile` aggregate with `EntityType`, `EntityId`, `Folder`, `FileName`, `StoredFileName`, `ContentType`, etc.
- A single `FileService` that wrote to `{ContentRoot}/AppData/{Folder}/{StoredFileName}`.
- A `FilesController` exposing `GET /api/files/{id}`, `/download`, `/{id}/stream`.
- Four upload commands (`UploadFile`, `UploadImage`, `UploadImages`, `UploadManyFiles`).
- A handful of `FluentValidation` validators (`FileSize`, `FileSignature`, `ImageExtension`, ...).

It compiled and worked. It just had several latent production hazards.

---

## 2. Defects in the old design

| # | Symptom | Why it's bad |
|---|---|---|
| 1 | `File.ReadAllBytesAsync(...)` in the download path | A 100 MB upload allocates a 100 MB `byte[]` per concurrent download. OOM risk, GC pressure, slow first byte. |
| 2 | `Path.Combine(_rootPath, file.Folder, ...)` with caller-supplied folder | Path-traversal: `folder = "..\\..\\Windows\\System32"` walks out of `AppData`. |
| 3 | Writes to disk then saves DB row separately | If the DB save fails, the on-disk blob is orphaned. |
| 4 | `Content-Disposition: inline; filename="{file.FileName}"` built by string interpolation | Non-ASCII names break, quotes can inject CRLF. |
| 5 | No `ETag`, no `Cache-Control` | Every page nav re-downloads every profile image. |
| 6 | `IFormFile.ContentType` was trusted for storage | The signature validator looked at real bytes, but the stored MIME came from the client. |
| 7 | `DeleteAsync` deleted on disk first, then DB. A failure between them leaves an orphan row pointing at a missing file. | Partial state. |
| 8 | No bulk operations | "Delete all files attached to this entity" required many round-trips. |
| 9 | No content-addressable identity | Re-uploading the same image created a duplicate physical blob. |
| 10 | Auth on download/stream was implicit (no attribute) | Either everything was public, or accidentally protected. |

Goal of the refactor: keep the same call sites working but fix all ten without making the code harder to reason about.

---

## 3. Target architecture

```
HTTP layer            FilesController
                         │
                         ▼  Wolverine bus
Application layer     IFileService           (Domain-aware: knows EntityType, etc.)
                         │
                         ▼
Infrastructure layer  IFileStorage           (Bytes only: SaveAsync / OpenReadAsync / DeleteAsync)
                         │
                         ▼
                      LocalFileStorage  ←→   S3FileStorage / BlobFileStorage (future)
```

Two interfaces, three concerns:

- **Controller** — HTTP semantics: ETag, Cache-Control, Range, Content-Disposition.
- **`IFileService`** — domain semantics: persistence as a unit of work, validation, dedup.
- **`IFileStorage`** — bytes in, bytes out. Nothing about the database, no ASP.NET types.

This is the **hexagonal / ports-and-adapters** pattern. The application layer talks to a port (`IFileStorage`); the adapter (filesystem, S3) plugs in at the edge.

---

## Step 1 — Add `IFileStorage` (pluggable physical storage)

```csharp
public interface IFileStorage : IScopedService
{
    Task<long> SaveAsync(string key, Stream content, CancellationToken ct = default);
    Task<Stream?> OpenReadAsync(string key, CancellationToken ct = default);
    Task<bool> DeleteAsync(string key, CancellationToken ct = default);
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);
}
```

### Why an abstraction at all?

Because today it's the local filesystem and tomorrow it's S3. The interface is intentionally **byte-oriented and key-addressed** — it knows nothing about `UploadedFile`, `EntityType`, or EF.

### Why `Stream` and not `byte[]`?

Streams flow chunk-by-chunk; arrays force a full allocation. The 64 KB read/write buffer in the local implementation is the only memory ever held for a file, no matter the size.

### Why `string?` return on `OpenReadAsync`?

Returning `null` is unambiguous for "no such key". Throwing on missing files would force every caller to wrap the call in try/catch just to handle the common case.

### Why `IScopedService`?

The codebase's auto-DI scanner registers any implementer with the matching lifetime. Marker interface = no manual registration line. (See [Concept reference: marker-interface DI](#marker-interface-di).)

---

## Step 2 — Implement `LocalFileStorage`

Two production-critical behaviours: **atomic writes** and **path-traversal defence**.

### Atomic write

```csharp
string tmp = absolute + ".tmp-" + Guid.NewGuid().ToString("N");

await using (var dest = new FileStream(
    tmp,
    FileMode.CreateNew,
    FileAccess.Write,
    FileShare.None,
    bufferSize: 64 * 1024,
    useAsync: true))
{
    await content.CopyToAsync(dest, ct);
    bytesWritten = dest.Length;
}

File.Move(tmp, absolute, overwrite: true);
```

Why each part:

- `FileMode.CreateNew` — fails if the temp name already exists. The GUID makes that essentially impossible, but the safety still costs nothing.
- `FileShare.None` — no other process or thread can read or write the temp while we're writing it.
- `bufferSize: 64 * 1024` — empirically a sweet spot for sequential I/O on modern disks.
- `useAsync: true` — async I/O uses overlapped I/O on Windows / aio on Linux. Without this the file stream's "async" methods just block a thread-pool thread.
- `File.Move(..., overwrite: true)` — atomic rename on the same volume on every supported platform. A reader either sees the previous version or the new version, never a half-written byte stream.

### Path-traversal defence

```csharp
private string ResolveSafe(string key)
{
    if (string.IsNullOrWhiteSpace(key))
        throw new ArgumentException("Key is required.", nameof(key));

    string normalized = key.Replace('\\', '/').TrimStart('/');
    string absolute = Path.GetFullPath(Path.Combine(_rootPath, normalized));

    if (!absolute.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
        throw new UnauthorizedAccessException("Key resolves outside storage root.");

    return absolute;
}
```

`Path.GetFullPath` collapses `..` segments. After collapsing, we check the absolute path is still a child of `_rootPath`. Any attempt to escape the root throws before we touch the filesystem.

What attacks this stops:

- `key = "..\\..\\..\\etc\\passwd"` — collapsed, no longer under root, rejected.
- `key = "/etc/passwd"` (rooted) — `Path.Combine` discards `_rootPath`, but the StartsWith check catches it.
- Backslash-vs-slash confusion on Linux — we normalise to `/` before the combine.

### Failure cleanup

```csharp
catch
{
    TryDelete(tmp);  // never re-throws
    throw;           // surfaces the original exception
}
```

If anything blows up between "started writing" and "rename", the partial temp file is removed best-effort. The caller still sees the real failure.

---

## Step 3 — Add `ContentHash` to `UploadedFile`

```csharp
public string ContentHash { get; private set; } = default!;

public static ErrorOr<UploadedFile> Create(
    /* ... */ , long fileSize, string contentHash, bool isPrimary = false)
{
    if (string.IsNullOrWhiteSpace(contentHash))
        return DomainErrors.GeneralErrors.InvalidState(
            nameof(UploadedFile), "ContentHash is required.");

    /* ... */
    return new UploadedFile(/* ... */, contentHash.Trim().ToLowerInvariant(), isPrimary);
}
```

### Why a content hash?

The SHA-256 of the bytes gives us three production wins from a single field:

1. **Strong HTTP ETag** — same content always yields the same tag, even after rename / metadata edits / storage swaps. Browsers use it to skip downloads.
2. **Dedup key** — two callers uploading the same file produce identical hashes, so a future "skip duplicate physical blob" check is a one-liner.
3. **Integrity audit** — re-hashing the disk and comparing to the row tells you whether a blob has been tampered with or corrupted.

### Why kept in the domain entity, not a converter?

Because we want the value to be present **the moment** the entity is created in memory. A converter only fires at materialisation time, so a brand-new in-memory entity would have a stale hash until it round-tripped through the DB.

---

## Step 4 — Update EF configuration + migration

Configuration:

```csharp
builder.Property(f => f.ContentHash)
    .IsRequired()
    .HasMaxLength(64);              // SHA-256 = 64 hex chars

builder.HasIndex(f => f.ContentHash);
```

Migration `20260515144234_UploadedFileContentHash`:

```csharp
migrationBuilder.AddColumn<string>(
    name: "ContentHash",
    table: "UploadedFiles",
    type: "character varying(64)",
    maxLength: 64,
    nullable: false,
    defaultValue: "");

migrationBuilder.CreateIndex(
    name: "IX_UploadedFiles_ContentHash",
    table: "UploadedFiles",
    column: "ContentHash");
```

### Why `defaultValue: ""` instead of backfilling?

To backfill correctly we'd need to read every existing blob from disk, hash it, and write it back. That requires application code (SQL can't compute SHA-256 of a file outside the DB) — so it's a one-off worker job, not a migration step.

The compromise: existing rows ship with an empty hash. The controller falls back to a **weak ETag** based on `id + length` for those rows, which is good enough to enable browser caching for the row's lifetime. Newly-uploaded rows get the strong tag immediately.

If full backfill is wanted, the worker is a few lines: enumerate `WHERE ContentHash = ''`, open the blob, hash it, save. Flag if you want me to write it.

---

## Step 5 — Streaming DTOs

```csharp
public sealed record FileDto(
    Stream Stream,
    string ContentType,
    string FileName,
    string ContentHash,
    long LengthInBytes);
```

The old `FileDto` carried `byte[] Content`. The new one carries an open `Stream`. Important rules of the road:

- Whoever produces the stream is no longer the owner. The controller's `FileStreamResult` will dispose the stream after the response is sent.
- For non-200 paths (e.g. a 304 short-circuit), the controller must dispose the stream itself.
- We carry `LengthInBytes` so the response can advertise `Content-Length`. Without it, the response is chunked even when we know the size — which breaks some progress UIs.

The old `StreamFileDto` and the separate `StreamFileQuery` / `StreamFileQueryHandler` / validator were deleted. Inline view and force-download both go through the same `OpenAsync` now.

---

## Step 6 — Redesign `IFileService`

```csharp
public interface IFileService : IScopedService
{
    Task<ErrorOr<UploadedFileInfo>> UploadAsync(
        string entityType, Guid entityId, string folder,
        IFormFile file, bool isPrimary = false, CancellationToken ct = default);

    Task<ErrorOr<UploadedFileInfo>> UploadAsync(
        UploadFileRequest request, CancellationToken ct = default);

    Task<ErrorOr<IReadOnlyList<UploadedFileInfo>>> UploadManyAsync(
        string entityType, Guid entityId, string folder,
        IFormFileCollection files, CancellationToken ct = default);

    Task<ErrorOr<FileDto>> OpenAsync(Guid id, CancellationToken ct = default);
    Task<ErrorOr<Success>> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ErrorOr<int>> DeleteManyAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
}
```

Notable changes:

- Two `UploadAsync` overloads: a convenience one for `IFormFile` (HTTP path), and a generic `UploadFileRequest` overload for non-HTTP callers (background ingester, internal scripts). One implementation, two doors.
- Returns `UploadedFileInfo(Guid FileId, string ContentHash, long SizeInBytes)` instead of bare `Guid` — so callers can dedup, build ETags, or surface "your upload is 2.4 MB" UI without a second query.
- `OpenAsync` replaced both `DownloadAsync` and `StreamAsync`. There was no actual difference between them — both pulled bytes out of storage. The HTTP layer decides inline vs attachment.
- `DeleteManyAsync` returns the count of deleted rows, useful for "removed N files" UI.

`UploadFileRequest` lives in `Application.Features.FilesFolder.Dtos`:

```csharp
public sealed record UploadFileRequest(
    string EntityType,
    Guid EntityId,
    string Folder,
    string OriginalFileName,
    string ContentType,
    Stream Content,
    bool IsPrimary = false);
```

---

## Step 7 — Implement the new `FileService`

The interesting parts (see `Expense_Tracker.Infrastructure/Services/FileService.cs`):

### Folder allow-list

```csharp
private static readonly Regex SafeFolderPattern =
    new("^[A-Za-z0-9._/-]+$", RegexOptions.Compiled);

if (string.IsNullOrWhiteSpace(request.Folder) ||
    !SafeFolderPattern.IsMatch(request.Folder))
    return DomainErrors.FileErrors.InvalidType("Folder name contains invalid characters.");
```

Defence in depth. `LocalFileStorage.ResolveSafe` already prevents escaping the root, but we also reject suspicious folder names at the application layer so the error reads better and the storage layer is reached only with normalised input.

### Streaming upload + hashing in one pass

```csharp
long size;
string hash;

await using (var hashing = new HashingStream(request.Content))
{
    try
    {
        size = await storage.SaveAsync(storageKey, hashing, ct);
    }
    catch (Exception ex)
    {
        return DomainErrors.FileErrors.UploadFailed(ex.Message);
    }

    hash = Convert.ToHexString(hashing.GetHashAndReset()).ToLowerInvariant();
}
```

The `HashingStream` is the trick: it wraps the inbound stream and feeds every chunk read into an `IncrementalHash` while the storage layer copies it to disk. No second pass over the file, no buffering.

### Rollback on domain validation failure

```csharp
ErrorOr<UploadedFile> domainResult = UploadedFile.Create(/* ... */);
if (domainResult.IsError)
{
    await storage.DeleteAsync(storageKey, CancellationToken.None);
    return domainResult.Errors;
}
```

If the domain object refuses to be created (e.g. zero size after streaming), we already have a blob on disk. Delete it before returning. `CancellationToken.None` because **cleanup must finish even if the caller cancelled** — partial state is worse than slow shutdown.

### Rollback on DB failure

```csharp
try
{
    await db.UploadedFiles.AddAsync(uploaded, ct);
    await db.SaveChangesAsync(ct);
}
catch
{
    await storage.DeleteAsync(storageKey, CancellationToken.None);
    throw;
}

return new UploadedFileInfo(uploaded.Id, uploaded.ContentHash, uploaded.FileSizeInBytes);
```

The two stores stay consistent. Either both have the file or neither does.

### `OpenAsync`

```csharp
UploadedFile? row = await db.UploadedFiles
    .AsNoTracking()
    .FirstOrDefaultAsync(f => f.Id == id, ct);

if (row is null)
    return DomainErrors.FileErrors.NotFound();

Stream? stream = await storage.OpenReadAsync(BuildKey(row.Folder, row.StoredFileName), ct);
if (stream is null)
    return DomainErrors.FileErrors.NotFound();   // row exists, blob doesn't (corruption / drift)

return new FileDto(stream, row.ContentType, row.FileName, row.ContentHash, row.FileSizeInBytes);
```

`AsNoTracking` is correct here — we're reading, never mutating the row.

### `DeleteAsync`

DB delete first, then blob:

```csharp
db.UploadedFiles.Remove(row);
await db.SaveChangesAsync(ct);

await storage.DeleteAsync(BuildKey(row.Folder, row.StoredFileName), CancellationToken.None);
```

Why this order: an orphan blob is harmless dead weight a sweep job can collect. An orphan row is a **lie** — it claims a file exists that you can't read.

---

## Step 8 — `HashingStream`

```csharp
internal sealed class HashingStream : Stream
{
    private readonly Stream _inner;
    private readonly IncrementalHash _hash;

    public HashingStream(Stream inner)
    {
        _inner = inner;
        _hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int read = _inner.Read(buffer, offset, count);
        if (read > 0) _hash.AppendData(buffer, offset, read);
        return read;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
    {
        int read = await _inner.ReadAsync(buffer, ct);
        if (read > 0) _hash.AppendData(buffer.Span[..read]);
        return read;
    }

    public byte[] GetHashAndReset() => _hash.GetHashAndReset();

    /* CanWrite/Length/Position/etc. */
}
```

### Why `IncrementalHash` and not `SHA256.HashData(...)`?

`SHA256.HashData` works on a single buffer or stream — but if you give it the stream, it reads the stream itself, leaving nothing for the storage layer to write. With `IncrementalHash` we feed it chunks as they're read by someone else.

### Why override both `Read` and `ReadAsync`?

`IFileStorage.SaveAsync` calls `Stream.CopyToAsync`, which internally calls `ReadAsync`. We override that for the actual hot path. The synchronous `Read` override is a defensive belt-and-braces in case some caller pulls bytes synchronously.

### Why `Span<byte>` over `byte[]`?

The async overload receives a `Memory<byte>`. `IncrementalHash.AppendData(ReadOnlySpan<byte>)` is the allocation-free variant. We slice with `[..read]` to feed only the bytes actually read this round.

---

## Step 9 — Update existing handlers

The four upload commands and the `GetFile` query continue to work — they just delegate to the new service.

`UploadImageCommandHandler`:

```csharp
public sealed class UploadImageCommandHandler(
    IFileService fileService,
    [FromKeyedServices("files")] IUrlBuilder fileUrlBuilder)
{
    public async Task<ErrorOr<UploadImageResponse>> Handle(UploadImageCommand request, CancellationToken ct)
    {
        var result = await fileService.UploadAsync(
            request.EntityType, request.EntityId, request.folder,
            request.Image, isPrimary: false, ct);

        if (result.IsError) return result.Errors;

        return new UploadImageResponse(result.Value.FileId, fileUrlBuilder.GetUrl(result.Value.FileId)!);
    }
}
```

`GetFileQueryHandler` collapses to one line:

```csharp
public sealed class GetFileQueryHandler(IFileService fileService)
{
    public Task<ErrorOr<FileDto>> Handle(GetFileQuery request, CancellationToken ct)
        => fileService.OpenAsync(request.Id, ct);
}
```

The old `StreamFileQuery`, `StreamFileQueryHandler`, and `StreamFileQueryValidator` were removed — all callers now go through `GetFileQuery` + `OpenAsync`.

---

## Step 10 — Delete command

```csharp
public sealed record DeleteFileCommand(Guid Id);

public sealed class DeleteFileCommandHandler(IFileService fileService)
{
    public Task<ErrorOr<Success>> Handle(DeleteFileCommand request, CancellationToken ct)
        => fileService.DeleteAsync(request.Id, ct);
}
```

Tiny by design. The interesting work (DB row + blob, in the right order, with cleanup) happens in `FileService`. The command/handler is just the message-bus surface.

---

## Step 11 — Production-grade `FilesController`

Three endpoints, one shared serving helper.

```csharp
[HttpGet("{id:guid}")]
[AllowAnonymous]
public Task<IActionResult> GetFile(Guid id, CancellationToken ct)
    => ServeAsync(id, asAttachment: false, ct);

[HttpGet("{id:guid}/download")]
[Authorize]
public Task<IActionResult> Download(Guid id, CancellationToken ct)
    => ServeAsync(id, asAttachment: true, ct);

[HttpDelete("{id:guid}")]
[Authorize]
public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
{
    var result = await bus.InvokeAsync<ErrorOr<Success>>(new DeleteFileCommand(id), ct);
    return result.Match<IActionResult>(_ => NoContent(), errs => this.Problem(errs));
}
```

### `ServeAsync`

```csharp
private async Task<IActionResult> ServeAsync(Guid id, bool asAttachment, CancellationToken ct)
{
    ErrorOr<FileDto> result = await bus.InvokeAsync<ErrorOr<FileDto>>(new GetFileQuery(id), ct);
    if (result.IsError) return this.Problem(result.Errors);

    FileDto file = result.Value;

    bool hasStrongHash = !string.IsNullOrEmpty(file.ContentHash);
    var etag = hasStrongHash
        ? new EntityTagHeaderValue($"\"{file.ContentHash}\"")
        : new EntityTagHeaderValue($"\"{id:N}-{file.LengthInBytes}\"", isWeak: true);

    if (Request.GetTypedHeaders().IfNoneMatch?
        .Any(t => t.Compare(etag, useStrongComparison: hasStrongHash)) == true)
    {
        file.Stream.Dispose();
        Response.Headers.ETag = etag.ToString();
        return StatusCode(StatusCodes.Status304NotModified);
    }

    Response.Headers.ETag = etag.ToString();
    Response.Headers.CacheControl = "private, max-age=86400, must-revalidate";

    var disposition = new ContentDispositionHeaderValue(asAttachment ? "attachment" : "inline")
    {
        FileNameStar = file.FileName,
    };
    Response.Headers.ContentDisposition = disposition.ToString();

    return new FileStreamResult(file.Stream, file.ContentType)
    {
        EnableRangeProcessing = true,
        FileDownloadName = asAttachment ? file.FileName : null,
    };
}
```

Things going on, in order:

1. **Strong vs weak ETag**. New rows have a SHA-256, so the strong form is used. Legacy rows (empty hash) fall back to a weak `W/"id-length"` tag — still cacheable, just won't survive a content rewrite.
2. **Conditional GET**. `If-None-Match` tells us "I already have this version." We respond `304 Not Modified` and drop the stream without sending the body. The client renders from its cache. `useStrongComparison: hasStrongHash` matches the kind of tag we just emitted.
3. **Cache-Control**. `private` (browser only, no shared CDN), `max-age=86400` (one day), `must-revalidate` (don't serve stale on network errors).
4. **Content-Disposition** built via `ContentDispositionHeaderValue`. The framework handles RFC 5987 quoting / `filename*=UTF-8''...` for non-ASCII characters and prevents header-injection.
5. **`FileStreamResult` + `EnableRangeProcessing`**. ASP.NET handles `Range: bytes=N-M` automatically for `<video>`, `<audio>`, and resumable downloads, returning `206 Partial Content`.
6. **Stream lifetime**. `FileStreamResult` disposes the stream after the response. We only need to dispose manually on the 304 path, where `FileStreamResult` is never created.

### Auth model

- `GET /api/files/{id}` is `[AllowAnonymous]` so `<img src="/api/files/{guid}">` works without an auth header.
- `GET /api/files/{id}/download` is `[Authorize]` to avoid drive-by attachment downloads.
- `DELETE /api/files/{id}` is `[Authorize]`. (You probably want to add a per-file ownership check; the call goes through the bus so an authorisation policy at the handler level fits cleanly.)

---

## Concept reference

This section is the standalone library of techniques the refactor uses. Skim it
the next time you see the same problem in a different feature.

### Streaming vs buffering

```csharp
// Buffering — ALL bytes allocated up-front. OOM on big files.
byte[] bytes = await File.ReadAllBytesAsync(path);
return File(bytes, contentType, name);

// Streaming — flows to the socket as it's read. Constant memory.
Stream s = new FileStream(path, FileMode.Open, FileAccess.Read,
    FileShare.Read, 64 * 1024, useAsync: true);
return new FileStreamResult(s, contentType);
```

`FileStreamResult` calls `stream.DisposeAsync()` in its result-execution pipeline — you don't need a `using`.

### Atomic file write — tmp + rename

`File.Move(src, dst, overwrite: true)` is a metadata-only operation on the same volume on every supported platform. It is observed atomically: a reader either sees the old file or the new one.

```csharp
string tmp = dest + ".tmp-" + Guid.NewGuid().ToString("N");
await using (var s = new FileStream(tmp, FileMode.CreateNew, FileAccess.Write,
    FileShare.None, 64 * 1024, useAsync: true))
{
    await source.CopyToAsync(s);
}
File.Move(tmp, dest, overwrite: true);
```

### Path-traversal defence

```csharp
string root = Path.GetFullPath("/var/app/data");
string candidate = Path.GetFullPath(Path.Combine(root, userInput));
if (!candidate.StartsWith(root))
    throw new UnauthorizedAccessException();
```

`Path.GetFullPath` collapses `.` and `..`. The StartsWith check guarantees the result is under the root no matter how cleverly `userInput` was crafted.

### `IncrementalHash`

```csharp
using var h = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
foreach (var chunk in chunks)
    h.AppendData(chunk);
byte[] digest = h.GetHashAndReset();
```

Lower allocation than `HashAlgorithm.TransformBlock`, supports `Span<byte>`, and lets multiple consumers (storage layer + hasher) share the same byte stream.

### HTTP `ETag` + `If-None-Match`

```http
GET /api/files/abc HTTP/1.1
If-None-Match: "9f8e7d6c..."

HTTP/1.1 304 Not Modified
ETag: "9f8e7d6c..."
```

The browser stores the previous tag and offers it back. The server compares and either returns `304` (no body) or `200` (full body + new tag). Strong ETag for content-hashed resources, weak (`W/"..."`) for "same enough but not byte-identical".

### `Cache-Control` directives, briefly

| Directive | Means |
|---|---|
| `private` | Only the user's browser may cache it (not a shared CDN/proxy). |
| `public` | Any cache may store it. |
| `max-age=N` | Fresh for N seconds. |
| `no-cache` | Cacheable, but must revalidate on every use. |
| `no-store` | Don't cache at all (use for sensitive data). |
| `must-revalidate` | Once stale, must re-check (never serve stale on network error). |
| `immutable` | The resource will never change at this URL — skip revalidation. |

### `enableRangeProcessing` and `Range:`

```http
GET /api/files/abc HTTP/1.1
Range: bytes=1024-2047

HTTP/1.1 206 Partial Content
Content-Range: bytes 1024-2047/9000000
```

`FileStreamResult.EnableRangeProcessing = true` makes ASP.NET handle this correctly. Required for `<video>` seeking and resumable downloads.

### RFC 5987 `Content-Disposition`

```
Content-Disposition: attachment; filename*=UTF-8''sm%C3%B6rg%C3%A5sbord.txt
```

Use the framework, never build the header by hand:

```csharp
var d = new ContentDispositionHeaderValue("inline") { FileNameStar = name };
Response.Headers.ContentDisposition = d.ToString();
```

### `CancellationToken.None` on cleanup paths

```csharp
try { await storage.SaveAsync(key, stream, ct); }
catch
{
    await storage.DeleteAsync(key, CancellationToken.None); // cleanup is uncancellable
    throw;
}
```

The original cancellation reason is already covered by the exception you'll re-throw. The cleanup must finish — leaving an orphan blob is worse than a slightly slower shutdown.

### Marker-interface DI

```csharp
public interface IScopedService { }
public interface ITransientService { }
public interface ISingletonService { }

services.Scan(scan =>
    scan.FromAssembliesOf(typeof(AppDbContext))
        .AddClasses(c => c.AssignableTo<IScopedService>())
        .AsImplementedInterfaces()
        .WithScopedLifetime());
```

A class implementing `IScopedService` is auto-registered with all of its non-marker interfaces. No manual `services.AddScoped<IFileStorage, LocalFileStorage>()` line.

### Content-addressable storage

A blob's identity is its content hash. Same input → same id. Used by Git (object store), Docker (image layers), IPFS, most CDN origins. Enables:

- Dedup ("you uploaded this file already, here's the existing id").
- Strong cache validators (the URL or row id can change, the hash can't).
- Integrity checks ("does what's on disk still match what was registered?").

### Ports and adapters (hexagonal)

The pattern this whole refactor uses:

- **Port** — `IFileStorage`, defined in the application layer. Plain types, no framework dependencies.
- **Adapter** — `LocalFileStorage`, in the infrastructure layer. Implements the port using a specific technology.
- **Application service** — `FileService`. Coordinates ports (storage + DB) and enforces invariants. Doesn't know which adapter is wired.

Swapping `LocalFileStorage` for `S3FileStorage` is a one-class change. The controller, the service, the EF row, the migration — all unchanged.

---

## Operational notes

### To deploy

```cmd
dotnet ef database update --project Expense_Tracker.Infrastructure --startup-project Expense_Tracker.App
```

This applies the `UploadedFileContentHash` migration (and any preceding ones).

### Backfill of `ContentHash`

Pre-existing rows ship with an empty `ContentHash`. A worker job is needed if you want them to switch from weak to strong ETags. Pseudocode:

```csharp
var rows = await db.UploadedFiles
    .Where(f => f.ContentHash == "")
    .ToListAsync();

foreach (var row in rows)
{
    using var s = await storage.OpenReadAsync(BuildKey(row.Folder, row.StoredFileName));
    if (s is null) continue;

    using var h = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
    var buf = ArrayPool<byte>.Shared.Rent(64 * 1024);
    int n;
    while ((n = await s.ReadAsync(buf)) > 0) h.AppendData(buf, 0, n);
    ArrayPool<byte>.Shared.Return(buf);

    row.SetContentHash(Convert.ToHexString(h.GetHashAndReset()).ToLowerInvariant());
}
await db.SaveChangesAsync();
```

(Setter would need to be exposed on the entity — only required if you decide to backfill.)

### Future: switch storage to S3

1. Add `Expense_Tracker.Infrastructure/Files/S3FileStorage.cs` implementing `IFileStorage`.
2. Replace the `LocalFileStorage` registration in `DependencyInjection.cs` (or branch on configuration).
3. Done. The controller, service, validators, migrations, and HTTP contract are unchanged.

---

## TL;DR

| Concern | Fix |
|---|---|
| OOM on download | `FileStreamResult` + 64 KB streaming, no `ReadAllBytesAsync`. |
| Path traversal | `LocalFileStorage.ResolveSafe` + folder regex. |
| Orphan blobs / rows | Tmp+rename write, rollback on validation/DB failure. |
| Header injection | `ContentDispositionHeaderValue.FileNameStar`. |
| No browser caching | Strong SHA-256 ETag + `Cache-Control: private, max-age=86400, must-revalidate`. |
| No range requests | `EnableRangeProcessing = true` on `FileStreamResult`. |
| Vendor lock-in | `IFileStorage` port, `LocalFileStorage` adapter, S3 plug-in any time. |
| Untrusted client MIME | Stored as a string, but signature validation runs on real bytes via existing validators. |
| Duplicate uploads | `ContentHash` indexed, dedup is a one-line query whenever you want it. |
| Mixed inline/download paths | One `OpenAsync`, controller chooses disposition. |
