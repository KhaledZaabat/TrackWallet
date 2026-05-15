# Production Refactor — Concepts and Tools

A walkthrough of the four refactors applied in this session, what was wrong before, what changed, and the .NET / Postgres / HTTP concepts each one uses. Everything has a runnable example so you can recognise the pattern next time.

---

## Table of contents

1. [CheckUsername optimisation](#1-checkusername-optimisation)
2. [NormalizedUserName — case-insensitive uniqueness](#2-normalizedusername--case-insensitive-uniqueness)
3. [Notifications — typed payload model](#3-notifications--typed-payload-model)
4. [File handling — streaming, atomic, content-addressable](#4-file-handling--streaming-atomic-content-addressable)
5. [Cross-cutting concepts cheat sheet](#5-cross-cutting-concepts-cheat-sheet)

---

## 1. CheckUsername optimisation

### Old

```csharp
public class CheckUsernameQueryHandler(IRepository<User> userRepo)
{
    public async Task<ErrorOr<UsernameAvailabilityResponse>> Handle(
        CheckUsernameQuery query, CancellationToken ct)
    {
        bool taken = await userRepo.Query()
            .AnyAsync(u => u.UserName == query.UserName, ct);
        return new UsernameAvailabilityResponse(!taken);
    }
}
```

Problems:
- Every keystroke from the SPA hits the database.
- Reserved names like `admin` are not blocked.
- Soft-deleted rows hide behind the global query filter, so a "free" username can fail at registration with a unique-index violation.
- Concurrent requests for the same name fire concurrent SQL queries (the **cache stampede** problem).

### New

Layered fast-paths plus a stampede-safe cache.

```csharp
[GeneratedRegex(@"^[a-zA-Z0-9._-]{3,50}$", RegexOptions.CultureInvariant)]
private static partial Regex UsernamePatternGenerated();

private static readonly FrozenSet<string> ReservedNormalized =
    new[] { "ADMIN", "ROOT", /* ... */ }.ToFrozenSet(StringComparer.Ordinal);

public async Task<ErrorOr<UsernameAvailabilityResponse>> Handle(
    CheckUsernameQuery query, CancellationToken ct)
{
    string? input = query.UserName?.Trim();

    // 1. Format gate — no DB, no cache.
    if (string.IsNullOrEmpty(input) || !UsernamePattern.IsMatch(input))
        return new UsernameAvailabilityResponse(false);

    string normalized = User.Normalize(input);

    // 2. Reserved gate.
    if (ReservedNormalized.Contains(normalized))
        return new UsernameAvailabilityResponse(false);

    // 3. Stampede-safe cache.
    string cacheKey = $"uname:{normalized}";
    Lazy<Task<bool>> lazy = cache.GetOrCreate(cacheKey, entry =>
    {
        entry.Size = 1;
        entry.AbsoluteExpirationRelativeToNow = AvailableTtl;
        return new Lazy<Task<bool>>(() =>
            CheckIsAvailableAsync(normalized, CancellationToken.None));
    })!;

    bool isAvailable = await lazy.Value.WaitAsync(ct).ConfigureAwait(false);

    // 4. Promote "taken" to a longer TTL — taken rarely flips back.
    if (!isAvailable)
        cache.Set(cacheKey, lazy, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TakenTtl,
            Size = 1,
        });

    return new UsernameAvailabilityResponse(isAvailable);
}
```

### Concepts

#### `[GeneratedRegex]` source generator

The compiler emits a specialised, allocation-free regex implementation at build time. Pattern is validated at compile time, and the engine doesn't need to interpret the pattern at run time.

```csharp
[GeneratedRegex(@"^\d{4}-\d{2}-\d{2}$")]
private static partial Regex IsoDate();

bool ok = IsoDate().IsMatch("2026-05-15"); // 0 allocations, fastest possible
```

Versus `new Regex(@"^\d{4}-\d{2}-\d{2}$")` which interprets the pattern at first use and allocates per call site.

#### `FrozenSet<T>`

A read-only set built once and tuned for lookups. Internally uses a perfect hash.

```csharp
using System.Collections.Frozen;

FrozenSet<string> stopWords = new[] { "the", "a", "an" }
    .ToFrozenSet(StringComparer.OrdinalIgnoreCase);

stopWords.Contains("THE"); // very fast, no per-call allocation
```

When to use: lookup-only collections that never change (config, blocklists, enums-as-strings). Don't use for collections you mutate.

#### Cache-stampede protection with `Lazy<Task<T>>`

Plain `IMemoryCache.GetOrCreateAsync(...)` does **not** prevent two concurrent callers from both invoking the factory at the same time when the entry is missing. Each caller fires its own DB query. With `Lazy<Task<bool>>` the lazy is created once, but the underlying task starts only when `.Value` is first read — every other caller awaits the same `Task` instance.

```csharp
// BAD — both callers miss the cache simultaneously, both run the factory.
var taken = await cache.GetOrCreateAsync(key, _ => DbCheckAsync());

// GOOD — at most one factory invocation, every caller awaits the same Task.
var lazy = cache.GetOrCreate(key, _ => new Lazy<Task<bool>>(DbCheckAsync));
bool taken = await lazy!.Value;
```

#### `Task.WaitAsync(CancellationToken)`

Waits for a task to finish but observes a separate cancellation token. The original task is **not** cancelled — only this caller's await stops. Crucial when many callers share one task: cancelling one shouldn't poison the others.

```csharp
Task<int> shared = SomeLongRunningCall();
int result = await shared.WaitAsync(ct);
// If ct fires, this caller throws OperationCanceledException, but `shared`
// keeps running for any other caller awaiting it.
```

#### EF Core `IgnoreQueryFilters()`

Bypasses global query filters (e.g., soft-delete `WHERE !IsDeleted`) for one query. Use it when the unique-index in the database covers all rows including soft-deleted, but a feature still needs to "see" them.

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b) =>
        b.HasQueryFilter(u => !u.IsDeleted);
}

// Default queries skip soft-deleted rows:
db.Users.Where(u => u.UserName == "alice").AnyAsync();           // sees only live rows

// Bypass for unique checks that mirror the DB index:
db.Users.IgnoreQueryFilters().Where(u => u.UserName == "alice"); // sees all rows
```

---

## 2. NormalizedUserName — case-insensitive uniqueness

### Old

`UserName` was indexed unique with default Postgres collation. `alice` and `Alice` were two different rows, but the SPA expected them to clash. Worse, the equality check `u.UserName == userName` produced an indexed seek but case-sensitive — so `Alice` searching for `alice` returned "available", and registration would then fail at INSERT.

### New

A second column `NormalizedUserName` (upper-invariant) maintained by the entity itself:

```csharp
public sealed class User : Entity
{
    public string UserName { get; private set; } = string.Empty;
    public string NormalizedUserName { get; private set; } = string.Empty;

    public static string Normalize(string userName) =>
        userName?.Trim().ToUpperInvariant() ?? string.Empty;

    private User(/*...*/, string userName, /*...*/)
    {
        UserName = userName;
        NormalizedUserName = Normalize(userName); // single source of truth
    }

    public ErrorOr<Success> UpdateUserName(string userName)
    {
        // ...validation
        UserName = userName.Trim();
        NormalizedUserName = Normalize(userName);
        return new Success();
    }
}
```

EF config:

```csharp
builder.Property(u => u.NormalizedUserName).IsRequired().HasMaxLength(50);
builder.HasIndex(u => u.NormalizedUserName).IsUnique();
```

Lookups use the normalised column:

```csharp
bool taken = await userRepo.Query()
    .IgnoreQueryFilters()
    .AnyAsync(u => u.NormalizedUserName == normalized, ct);
```

### Concepts

#### Domain invariant kept inside the entity

The class can never be in a state where `NormalizedUserName` disagrees with `UserName`. Constructors and the only mutator both call `Normalize`. There is no "set this column from outside" path.

This is safer than an EF `ValueConverter` because converters only run at materialisation, so a freshly-created in-memory entity would have an empty normalised value until it round-tripped through the DB.

#### Indexed case-insensitive search without `citext`

Postgres has a `citext` extension and `ILIKE`, but they're either an extension or non-indexed. Storing the upper-invariant explicitly gives you:

- A normal `btree` unique index — index seeks, not scans.
- No extension dependency.
- Works identically across SQL Server, Postgres, SQLite.

#### `ToUpperInvariant` vs `ToLower`

`ToUpperInvariant` is the recommended choice for case folding because it round-trips more characters correctly across cultures. ASP.NET Identity uses the same convention for `NormalizedUserName`.

```csharp
"İ".ToUpperInvariant(); // "İ" — round-trips
"İ".ToLowerInvariant(); // "i̇" — adds a combining mark
```

#### Hand-edited migration — three-step backfill

When you add a NOT NULL column to a non-empty table you cannot add it and immediately apply a unique index — every row would get the same default and violate uniqueness. The pattern:

```csharp
protected override void Up(MigrationBuilder mb)
{
    mb.DropIndex("IX_Users_UserName", "Users");

    mb.AddColumn<string>("NormalizedUserName", "Users",
        type: "character varying(50)", nullable: true); // step 1: nullable

    mb.Sql(@"UPDATE ""Users"" SET ""NormalizedUserName"" = UPPER(""UserName"");"); // step 2: backfill

    mb.AlterColumn<string>("NormalizedUserName", "Users",
        type: "character varying(50)", nullable: false); // step 3: lock down

    mb.CreateIndex("IX_Users_NormalizedUserName", "Users",
        "NormalizedUserName", unique: true);
}
```

---

## 3. Notifications — typed payload model

### Old

```csharp
DomainNotification.Create(
    userId: invitation.InviteeUserId,
    title: "👨‍👩‍👧‍👦 New family invitation",
    body: $"{inviterName} invited you to {familyName}",
    type: NotificationType.FamilyInvitation,
    data: new Dictionary<string, string>
    {
        ["invitation_id"] = invitation.Id.ToString(),
        ["family_id"]     = invitation.FamilyId.ToString(),
        // every handler invented its own keys
    });
```

Problems:
- Hard-coded titles in five handlers — every copy edit touched five files.
- `Dictionary<string, string>` is a contract by convention. The SPA had to know magic key names per type.
- No deep-link, no severity, no category. SPA couldn't render rich cards.
- Email + push was glued into the same handler. Mixed concerns.
- Repository's `MarkAsRead` mutated the entity but didn't `SaveChanges`.

### New

#### Discriminated union via `System.Text.Json` polymorphism

```csharp
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$kind")]
[JsonDerivedType(typeof(FamilyInvitationPayload), "family-invitation")]
[JsonDerivedType(typeof(InvitationAcceptedPayload), "invitation-accepted")]
[JsonDerivedType(typeof(TransactionCreatedPayload), "transaction-created")]
public abstract record NotificationPayload;

public sealed record FamilyInvitationPayload(
    Guid InvitationId, Guid FamilyId, string FamilyName,
    Guid InviterUserId, string InviterUserName) : NotificationPayload;
```

When this is serialised it produces:

```json
{
  "$kind": "family-invitation",
  "invitationId": "...",
  "familyId": "...",
  "familyName": "Smiths",
  "inviterUserId": "...",
  "inviterUserName": "alice"
}
```

The SPA can `switch (payload.$kind)` and TypeScript will narrow the type.

#### Single `INotificationTemplateRegistry`

All title / body / icon / severity / category / deep-link decisions live in one place:

```csharp
public NotificationTemplate Render(NotificationPayload payload) => payload switch
{
    FamilyInvitationPayload p => new NotificationTemplate(
        Title: "New family invitation",
        Body: $"{p.InviterUserName} invited you to join {p.FamilyName}.",
        IconKey: NotificationIcons.FamilyInvitation,
        Category: NotificationCategory.Family,
        Severity: NotificationSeverity.Info,
        ResourceUri: $"/invitations/{p.InvitationId}"),
    // ...
};
```

#### `INotificationBuilder` — what handlers actually call

```csharp
DomainNotification n = notificationBuilder.Build(
    recipientUserId: invitation.InviteeUserId,
    actorUserId: invitation.InviterUserId,
    payload: new FamilyInvitationPayload(/*...*/));

await dispatcher.EnqueueAsync(n, ct);
```

Handlers describe **what happened** with a typed payload. The builder + registry decide **how it appears**.

#### Industry-standard fields on the entity

```csharp
public sealed class DomainNotification : Entity, ICreatable
{
    public Guid UserId { get; private set; }
    public Guid? ActorUserId { get; private set; }
    public NotificationType Type { get; private set; }
    public NotificationCategory Category { get; private set; }
    public NotificationSeverity Severity { get; private set; }
    public string Title { get; private set; }
    public string Body { get; private set; }
    public string IconKey { get; private set; }
    public string? ResourceUri { get; private set; }       // deep-link
    public NotificationPayload? Payload { get; private set; } // typed JSONB
    public bool IsRead { get; private set; }
    public DateTimeOffset? ReadAtUtc { get; private set; }
    /* ... */
}
```

#### SPA-facing API

```
GET    /api/notifications?onlyUnread&skip&take
GET    /api/notifications/unread-count
POST   /api/notifications/{id}/read
POST   /api/notifications/read-all
```

#### Bulk update with `ExecuteUpdateAsync`

Mark-all-as-read previously would have loaded every unread row into memory and saved each one. The right way is to issue a single SQL `UPDATE`:

```csharp
return await db.QueryTracked()
    .Where(n => n.UserId == userId && !n.IsRead)
    .ExecuteUpdateAsync(s => s
        .SetProperty(n => n.IsRead, true)
        .SetProperty(n => n.ReadAtUtc, now), ct);
```

EF Core 7+ feature — translates to a single SQL statement, no rows materialised, no change tracker involvement.

### Concepts

#### Postgres `jsonb` + EF `ValueConverter`

JSONB stores parsed JSON binary, supports operators (`->`, `->>`, `@>`) and GIN indexes. Bound to a property via a converter:

```csharp
builder.Property(x => x.Payload)
    .HasColumnType("jsonb")
    .HasConversion(
        v => JsonSerializer.Serialize(v, opts),
        v => JsonSerializer.Deserialize<NotificationPayload>(v, opts));
```

#### Partial indexes

Postgres can index only rows that match a predicate. A "list unread notifications" query is the hot path; indexing only unread rows keeps the index tiny:

```csharp
builder.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAtUtc })
    .HasFilter("\"IsRead\" = false");
```

```sql
CREATE INDEX IX_Notifications_UserId_Unread
    ON "Notifications" ("UserId", "IsRead", "CreatedAtUtc")
    WHERE "IsRead" = false;
```

#### Descending index

Lists are typically newest-first. Make the index match the sort to enable an index-only scan:

```csharp
builder.HasIndex(x => new { x.UserId, x.CreatedAtUtc })
    .IsDescending(false, true); // UserId asc, CreatedAtUtc desc
```

---

## 4. File handling — streaming, atomic, content-addressable

### Old (a tour of the bugs)

```csharp
public async Task<ErrorOr<FileDto>> DownloadAsync(Guid id, CancellationToken ct)
{
    var file = await db.UploadedFiles.FirstOrDefaultAsync(f => f.Id == id, ct);
    var path = Path.Combine(_rootPath, file.Folder, file.StoredFileName);
    var bytes = await System.IO.File.ReadAllBytesAsync(path, ct); // ← OOM on large files
    return new FileDto(bytes, file.ContentType, file.FileName);
}
```

Issues, top to bottom:
- `ReadAllBytesAsync` loads the whole file into a `byte[]`. A 100 MB upload is 100 MB of allocations per concurrent download.
- `Path.Combine(_rootPath, file.Folder, ...)` with a caller-supplied `folder` is a path-traversal hole — `..\..\Windows\System32` walks out of `AppData`.
- File saved to disk first, then DB row inserted. If the DB save fails, the blob orphan stays forever.
- Caller `ContentType` was trusted, even though a `FileSignatureValidator` had inspected real bytes.
- No HTTP cache — every `<img>` re-fetched on every page load.
- `Content-Disposition: inline; filename="{file.FileName}"` — string interpolation breaks for non-ASCII names and is technically header-injectable.

### New

#### Layered architecture

```
HTTP → FilesController → IFileService (Application) → IFileStorage (Infrastructure)
                                                          ↑
                                       LocalFileStorage today, S3 tomorrow.
```

#### `IFileStorage` — one interface, swappable backends

```csharp
public interface IFileStorage : IScopedService
{
    Task<long> SaveAsync(string key, Stream content, CancellationToken ct = default);
    Task<Stream?> OpenReadAsync(string key, CancellationToken ct = default);
    Task<bool> DeleteAsync(string key, CancellationToken ct = default);
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);
}
```

`LocalFileStorage` is the only implementation today. Swapping to S3 is a one-class change; the entire domain, application and HTTP layers don't move.

#### Atomic write via tmp + rename

```csharp
string tmp = absolute + ".tmp-" + Guid.NewGuid().ToString("N");
await using (var dest = new FileStream(tmp, FileMode.CreateNew, FileAccess.Write,
    FileShare.None, bufferSize: 64 * 1024, useAsync: true))
{
    await content.CopyToAsync(dest, ct);
}
File.Move(tmp, absolute, overwrite: true); // atomic on the same volume
```

A reader either sees no file, or the fully-written file. There is no "half-written" intermediate visible state.

#### Path-traversal safety

```csharp
private string ResolveSafe(string key)
{
    string normalized = key.Replace('\\', '/').TrimStart('/');
    string absolute = Path.GetFullPath(Path.Combine(_rootPath, normalized));

    if (!absolute.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
        throw new UnauthorizedAccessException("Key resolves outside storage root.");

    return absolute;
}
```

`Path.GetFullPath` collapses `..` segments. We then verify the result is still inside the root. A caller passing `..\..\..\etc\passwd` resolves to a path outside `AppData/` and is rejected before any filesystem call.

#### Atomic upload + DB row

```csharp
size = await storage.SaveAsync(storageKey, hashing, ct);

ErrorOr<UploadedFile> domainResult = UploadedFile.Create(/*..., contentHash, ...*/);
if (domainResult.IsError)
{
    await storage.DeleteAsync(storageKey, CancellationToken.None); // rollback
    return domainResult.Errors;
}

try
{
    await db.UploadedFiles.AddAsync(domainResult.Value, ct);
    await db.SaveChangesAsync(ct);
}
catch
{
    await storage.DeleteAsync(storageKey, CancellationToken.None); // rollback
    throw;
}
```

Either both the row and the blob exist, or neither does. Crashes mid-save leave nothing dangling.

#### Streaming hash with `IncrementalHash`

The file is hashed **as it streams** to disk. No second pass, no full-file load.

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

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
    {
        int read = await _inner.ReadAsync(buffer, ct);
        if (read > 0) _hash.AppendData(buffer.Span[..read]);
        return read;
    }

    public byte[] GetHashAndReset() => _hash.GetHashAndReset();
    /* ... */
}
```

Every byte the storage layer reads also flows into the hasher. After the stream is exhausted, calling `GetHashAndReset()` produces the final SHA-256.

#### HTTP-grade controller

```csharp
[HttpGet("{id:guid}")]
[AllowAnonymous]
public async Task<IActionResult> GetFile(Guid id, CancellationToken ct)
{
    FileDto file = (await bus.InvokeAsync<ErrorOr<FileDto>>(new GetFileQuery(id), ct)).Value;

    var etag = new EntityTagHeaderValue($"\"{file.ContentHash}\"");

    if (Request.GetTypedHeaders().IfNoneMatch?.Any(t => t.Compare(etag, true)) == true)
    {
        file.Stream.Dispose();
        Response.Headers.ETag = etag.ToString();
        return StatusCode(StatusCodes.Status304NotModified);
    }

    Response.Headers.ETag = etag.ToString();
    Response.Headers.CacheControl = "private, max-age=86400, must-revalidate";

    var disposition = new ContentDispositionHeaderValue("inline")
    {
        FileNameStar = file.FileName, // RFC 5987 — non-ASCII safe
    };
    Response.Headers.ContentDisposition = disposition.ToString();

    return new FileStreamResult(file.Stream, file.ContentType)
    {
        EnableRangeProcessing = true,
    };
}
```

### Concepts

#### Streaming vs buffering

```csharp
// Buffering — every byte allocated, OOM-prone, slow to first byte.
byte[] bytes = await File.ReadAllBytesAsync(path);
return File(bytes, contentType, name);

// Streaming — flows to socket as it's read, constant memory, fast first byte.
Stream s = new FileStream(path, FileMode.Open, FileAccess.Read,
    FileShare.Read, 64 * 1024, useAsync: true);
return new FileStreamResult(s, contentType);
```

`FileStreamResult` disposes the stream after the response is fully sent. You don't need a `using` around it.

#### Atomic file write — tmp + rename

`File.Move(src, dst, overwrite: true)` is atomic on the same volume on every supported platform. Combined with `FileMode.CreateNew` on the temp file (fails if it exists), you get a write that cannot leave a half-formed destination.

```csharp
string tmp = dest + ".tmp";
using (var s = new FileStream(tmp, FileMode.CreateNew, FileAccess.Write,
    FileShare.None, 64 * 1024, useAsync: true))
{
    await source.CopyToAsync(s);
}
File.Move(tmp, dest, overwrite: true);
```

#### Path-traversal defence

Whenever a path is built from caller input, normalise and verify:

```csharp
string root = Path.GetFullPath("/var/app/data");
string candidate = Path.GetFullPath(Path.Combine(root, userInput));
if (!candidate.StartsWith(root)) throw new UnauthorizedAccessException();
```

Without this, `userInput = "../../etc/passwd"` happily reads `/etc/passwd`.

#### `IncrementalHash`

Computes a hash over chunks. Works with any `HashAlgorithmName` (SHA-256, MD5, ...). Lower allocation than `HashAlgorithm.TransformBlock` and supports `Span<byte>`.

```csharp
using var h = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
foreach (var chunk in chunks) h.AppendData(chunk);
byte[] digest = h.GetHashAndReset();
```

#### HTTP `ETag` + conditional GET

An ETag is a server-generated identifier for a specific version of a resource. The client stores it and on the next request sends `If-None-Match: "tag"`. The server compares and returns either `200 OK` with the body or `304 Not Modified` with no body.

```http
GET /api/files/abc HTTP/1.1
If-None-Match: "9f8e7d6c..."

HTTP/1.1 304 Not Modified
ETag: "9f8e7d6c..."
```

For files we use the SHA-256 as the strong ETag. Same content → same tag, even after re-uploading or rotating storage.

#### `Cache-Control: private, max-age=N, must-revalidate`

- `private`: only the user's browser may cache it (not a shared CDN or proxy).
- `max-age=N`: fresh for N seconds without revalidating.
- `must-revalidate`: once stale, must re-check (won't serve stale on network errors).

Profile images and static documents typically use this.

#### `enableRangeProcessing` and `Range:`

Browsers and video players send `Range: bytes=N-M` to fetch slices. `FileStreamResult.EnableRangeProcessing = true` handles `206 Partial Content` for free, which is essential for resumable downloads and `<video>` seeking.

#### RFC 5987 filenames

```
Content-Disposition: attachment; filename*=UTF-8''sm%C3%B6rg%C3%A5sbord.txt
```

`ContentDispositionHeaderValue.FileNameStar` produces this automatically. Avoid building the header by hand:

```csharp
// BAD — breaks for "résumé.pdf" or anything with quotes.
Response.Headers.ContentDisposition = $"inline; filename=\"{name}\"";

// GOOD — escapes correctly.
var d = new ContentDispositionHeaderValue("inline") { FileNameStar = name };
Response.Headers.ContentDisposition = d.ToString();
```

#### Content-addressable storage

Two identical uploads produce identical SHA-256 digests. With `ContentHash` indexed, "skip the upload, point at the existing blob" is one query. Pattern used by Git, Docker layers, IPFS, and most CDN origins.

---

## 5. Cross-cutting concepts cheat sheet

### `ErrorOr<T>`

A discriminated union over `T` and a list of errors. Replaces both exceptions for control flow and the "return null on failure" anti-pattern.

```csharp
public async Task<ErrorOr<User>> GetAsync(Guid id)
{
    var u = await db.Users.FindAsync(id);
    if (u is null) return DomainErrors.UserErrors.NotFound();
    return u;
}

ErrorOr<User> r = await GetAsync(id);
if (r.IsError) return r.Errors;
User user = r.Value;
```

In a controller, `r.Match<IActionResult>(v => Ok(v), errs => this.Problem(errs))`.

### Marker interfaces for DI

```csharp
public interface IScopedService { }
public interface ITransientService { }
public interface ISingletonService { }
```

A class implementing one of these is auto-registered by the DI scanner with the corresponding lifetime — no manual `AddScoped<T>(...)` per service.

```csharp
services.Scan(scan =>
    scan.FromAssembliesOf(typeof(AppDbContext))
        .AddClasses(c => c.AssignableTo<IScopedService>())
        .AsImplementedInterfaces()
        .WithScopedLifetime());
```

### Auto-completion of audit fields via interceptors

Entities that implement `ICreatable`, `IUpdatable`, `ISoftDeletable` get their audit fields populated by EF interceptors registered globally. You don't write `entity.CreatedAtUtc = DateTime.UtcNow` in handlers.

### Wolverine `IMessageBus`

A mediator-style command/query bus. `bus.InvokeAsync<TResponse>(command, ct)` finds the registered handler and invokes it.

```csharp
public sealed record CreateInvoiceCommand(Guid CustomerId, decimal Amount);

public sealed class CreateInvoiceCommandHandler
{
    public Task<ErrorOr<Guid>> Handle(CreateInvoiceCommand cmd, CancellationToken ct)
        => /* ... */;
}

// Caller:
var id = await bus.InvokeAsync<ErrorOr<Guid>>(new CreateInvoiceCommand(c, a), ct);
```

### EF Core `Query()` vs `QueryTracked()`

`Query()` returns `AsNoTracking` — for reads. `QueryTracked()` returns the tracked queryable — for "load, mutate, save" workflows. Tracking is expensive; use `Query()` whenever you only need to read.

### `ExecuteUpdateAsync` / `ExecuteDeleteAsync`

Bulk DML in a single SQL statement, no change-tracker, no row materialisation.

```csharp
await db.Notifications
    .Where(n => n.UserId == userId && !n.IsRead)
    .ExecuteUpdateAsync(s => s
        .SetProperty(n => n.IsRead, true)
        .SetProperty(n => n.ReadAtUtc, DateTimeOffset.UtcNow), ct);
```

### `CancellationToken.None` vs the inbound `ct`

When you're rolling back something on the failure path (deleting an orphan blob, releasing a lock), pass `CancellationToken.None`. The cleanup must finish even if the caller has cancelled. The original cancel reason is already covered by the exception you'll re-throw.

```csharp
try { await storage.SaveAsync(key, stream, ct); }
catch
{
    await storage.DeleteAsync(key, CancellationToken.None); // never cancellable
    throw;
}
```

### `[FromKeyedServices("name")]`

Resolve a specific named registration when an interface has multiple implementations:

```csharp
services.AddKeyedScoped<IUrlBuilder, FileUrlBuilder>("files", /*factory*/);

public sealed class GetMeQueryHandler(
    [FromKeyedServices("files")] IUrlBuilder fileUrlBuilder)
{ /* ... */ }
```

### `[GeneratedRegex]` quick re-cap

Source-generator-emitted regex. Requires the containing class to be `partial`. Zero allocations per match, full pattern validation at compile time.

### `FrozenSet<T>` / `FrozenDictionary<TKey, TValue>` quick re-cap

Read-only, lookup-tuned collections. Build once via `.ToFrozenSet()` / `.ToFrozenDictionary()`. Use for static blocklists, type→handler maps, MIME registries, etc.

---

## TL;DR table

| Refactor | Old | New | Headline win |
|---|---|---|---|
| CheckUsername | Hits DB on every keystroke | Format → reserved → cache → DB, all stampede-safe | DB load drops by orders of magnitude |
| NormalizedUserName | Case-sensitive index, false positives | Index on upper-invariant column, kept in sync by entity | Correct uniqueness, indexed lookups |
| Notifications | Stringly-typed Dictionary, hard-coded titles | Discriminated-union JSONB payload + central template registry | One file owns rendering, SPA gets typed payload |
| File handling | Loads whole file in RAM, no atomicity, no caching | Streamed, atomic tmp+rename, SHA-256 ETag, RFC-5987, auth | OOM-safe, browser-cacheable, swappable storage |
