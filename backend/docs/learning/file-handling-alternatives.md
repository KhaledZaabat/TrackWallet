# Why not the simpler approaches? — an honest rundown

You suggested three lighter alternatives:

1. `UseStaticFiles()` / `MapStaticAssets()` — let the framework serve files from disk.
2. Plain `File(stream, ct, name, enableRangeProcessing: true)` + `[ResponseCache]`.
3. A middleware that adds ETag headers globally.

This doc walks through each one, says what it actually does and where it
breaks, then ends with the **simplification I should have done from the
start** — using the built-in `File(stream, …, lastModified, etag, …)`
overload, which does most of what my hand-rolled code does but in one line.

---

## Option 1 — `UseStaticFiles()` / `MapStaticAssets()`

### What it is

You expose a folder on disk under a URL prefix:

```csharp
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "AppData")),
    RequestPath = "/files",
});
```

The middleware then handles `GET /files/profiles/abc123.jpg` by streaming
`{ContentRoot}/AppData/profiles/abc123.jpg` straight to the socket. It does:

- Range requests
- `Last-Modified` + `If-Modified-Since`
- A weak `ETag` derived from length + last-write timestamp
- 304 short-circuit

`MapStaticAssets()` (added in .NET 9) is the modern replacement for the
build-time fingerprinted asset pipeline. Same idea, faster, optimised for
build-time-known files.

### Why I didn't use it

There are five blockers for this codebase, in order of severity:

#### 1. Authorisation is per-file, not per-folder

Some files (profile avatars) are public; others (private uploads, family
documents) need `[Authorize]` and even per-row checks ("is this attachment
on a transaction the caller can see?"). `UseStaticFiles` is a flat middleware
that either serves or doesn't — it has no idea what an entity is.

You can graft auth on top, but you end up doing this:

```csharp
app.MapWhen(
    ctx => ctx.Request.Path.StartsWithSegments("/files"),
    branch => branch.UseRouting()
                    .UseAuthentication()
                    .UseAuthorization()
                    .UseMiddleware<PerFileAuthMiddleware>()
                    .UseStaticFiles(...));
```

…and now you've reinvented an `[Authorize]`-aware controller, just less
discoverable.

#### 2. URLs leak the on-disk layout

`UseStaticFiles` maps URL → file path. If our storage scheme is
`AppData/Profiles/{guid}.jpg`, the URL is `/files/Profiles/{guid}.jpg`. The
SPA now knows folder conventions, GUID-as-filename, and the existence of
specific files. Move to S3 and every `<img src>` in the app breaks.

The current design hides all of that behind `/api/files/{id}`. The id is the
DB row, not the path; storage moves freely under it.

#### 3. Soft-deletes can't lie

Suppose a user soft-deletes an attachment. The DB row is gone (or flagged),
but the blob is still on disk. `UseStaticFiles` happily serves it because it
doesn't read the DB. You'd be one URL guess away from leaking deleted data.

The controller route asks the DB first, sees the row missing, and returns
404. That's the whole point of indirection through the database.

#### 4. No content-addressable identity

`UseStaticFiles`'s ETag is `"length-lastWriteUtc"`. If you migrate a file
to a new disk (different mtime), the ETag changes even though the bytes
didn't. Browsers re-download. Same problem if the OS rounds mtimes
differently across filesystems (NTFS vs ext4 vs APFS).

The SHA-256 is content-addressable: same bytes, same tag, forever, anywhere.

#### 5. Multi-instance / load-balanced setups

`UseStaticFiles` reads from local disk. Two app instances behind a load
balancer means two disks. Either you mount a shared filesystem (NFS, SMB —
fun in production), or you accept that "which instance handled the upload"
decides "which instance can serve the file." That's a coin flip per request.

The `IFileStorage` abstraction makes the swap to a shared backend (S3,
Azure Blob) a single class change. The controller, the service, the
migration — all unchanged.

### Where `UseStaticFiles` is the right call

It is excellent for:

- Built-time assets (CSS, JS, fonts, the SPA itself).
- Public, anonymous content where the URL *is* the canonical identifier
  (a marketing page's hero image).
- Single-instance deployments with no auth requirements.

For domain-owned, audited, optionally-soft-deleted, ACL'd uploads, it's
the wrong layer.

---

## Option 2 — `File(stream, ct, name, enableRangeProcessing: true)` + `[ResponseCache]`

### What it is

The minimal-controller version:

```csharp
[HttpGet("{id:guid}")]
[ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Client)]
public async Task<IActionResult> GetFile(Guid id, CancellationToken ct)
{
    var file = (await bus.InvokeAsync<ErrorOr<FileDto>>(new GetFileQuery(id), ct)).Value;
    return File(file.Stream, file.ContentType, file.FileName, enableRangeProcessing: true);
}
```

### What it actually does

| Concern | Handled? | Notes |
|---|---|---|
| Range requests | ✓ | `enableRangeProcessing: true` is enough. |
| `Content-Length` | ✓ | Set from the stream's `Length`. |
| `Content-Disposition` | ✓ | RFC 5987 quoting included automatically. |
| `Cache-Control` from `[ResponseCache]` | ✓ | Sets the response header. |
| ETag | ✗ | **No ETag is generated.** |
| `If-None-Match` → 304 | ✗ | **No.** |
| `If-Modified-Since` → 304 | ✗ | Not without `LastModified`. |
| `If-Range` for partial content | ✗ | Not without an ETag/LastModified. |

Two important gotchas:

#### `[ResponseCache]` does not generate ETags

`[ResponseCache]` only sets the `Cache-Control` and `Vary` response headers.
It does **nothing** about validators (`ETag`, `Last-Modified`). The browser
caches for `Duration` seconds — when `must-revalidate` fires, there's no
validator to round-trip with, so the browser re-downloads in full. You lose
the 304 optimisation entirely.

#### `[ResponseCache]` can also enable server-side caching

If you ever add `services.AddResponseCaching()` and `app.UseResponseCaching()`,
`[ResponseCache]` will start caching the response **on the server**. For
user-specific files this is dangerous — the response cache key doesn't know
about cookies by default, so user A's file can be served from cache to user
B. There are mitigations (`VaryByHeader`, `VaryByQueryKeys`, `Location =
ResponseCacheLocation.Client`), but it's a footgun you have to know about.

### Where this option is fine

- One-off files where 304 doesn't matter (a single rare download).
- Background tasks streaming small responses where re-fetching is cheap.

For a SPA where every page nav fetches several `<img>` tags, losing 304
support means losing the cheap-cache hit and turning each return visit into
a fresh download.

---

## Option 3 — Middleware that adds ETags globally

### What's possible

ASP.NET Core does not ship an "auto-ETag from response body" middleware.
Some communities have written third-party ones (e.g. `Microsoft.AspNetCore.HeaderPropagation`-style packages on GitHub) that buffer the response body, hash it, write the ETag, and short-circuit on `If-None-Match`.

The mechanics:

```csharp
app.Use(async (ctx, next) =>
{
    using var ms = new MemoryStream();
    var original = ctx.Response.Body;
    ctx.Response.Body = ms;

    await next();

    ms.Position = 0;
    var hash = SHA256.HashData(ms.ToArray());
    var etag = $"\"{Convert.ToHexString(hash).ToLower()}\"";

    if (ctx.Request.Headers.IfNoneMatch == etag)
    {
        ctx.Response.StatusCode = 304;
        ctx.Response.Body = original;
        return;
    }

    ctx.Response.Headers.ETag = etag;
    ms.Position = 0;
    await ms.CopyToAsync(original);
});
```

### Why this is a bad fit for files

- It **buffers the entire response into memory** to hash it. Defeats the
  point of streaming — a 100 MB download allocates 100 MB.
- It hashes *every* response, not just files. CSS, JSON APIs, everything.
- Range responses break: you've already lost the original Content-Length
  and accept-ranges semantics.
- The ETag changes whenever the *response* changes (headers, framing),
  not just when the *content* changes.

There's a real reason CDNs do this with a content-aware fast path and a
streaming Merkle hash — and we're not building a CDN.

### Where middleware *does* help

Adding `Cache-Control` headers globally based on path patterns (no body
inspection):

```csharp
app.Use(async (ctx, next) =>
{
    await next();
    if (ctx.Request.Path.StartsWithSegments("/api/files"))
        ctx.Response.Headers.CacheControl = "private, max-age=86400, must-revalidate";
});
```

That's a no-cost win and could move the `Cache-Control` line out of the
controller. We could do that.

---

## What I should actually have done

There's a **fourth option** that combines the framework's own machinery
with the SHA-256 we already compute in the service: the
`File(stream, contentType, fileDownloadName, lastModified, entityTag, enableRangeProcessing)`
overload.

```csharp
File(
    fileStream: file.Stream,
    contentType: file.ContentType,
    fileDownloadName: asAttachment ? file.FileName : null,
    lastModified: null,
    entityTag: new EntityTagHeaderValue($"\"{file.ContentHash}\""),
    enableRangeProcessing: true);
```

When you give the framework an `EntityTagHeaderValue`, it handles **all of
the conditional-GET logic for you**:

- Sends `ETag: "<hash>"` on the response.
- Honours `If-None-Match` → returns `304` with no body.
- Honours `If-Modified-Since` → returns `304`.
- Honours `If-Range` → falls back to `200` if the validator doesn't match.
- Range processing still works.
- `Content-Disposition` (RFC 5987 + ASCII fallback) is set automatically
  from `fileDownloadName`.

That collapses my hand-written `If-None-Match` block and `ContentDisposition`
construction into the overload's parameters.

### My code becomes

```csharp
private async Task<IActionResult> ServeAsync(Guid id, bool asAttachment, CancellationToken ct)
{
    var result = await bus.InvokeAsync<ErrorOr<FileDto>>(new GetFileQuery(id), ct);
    if (result.IsError) return this.Problem(result.Errors);

    var file = result.Value;

    var etag = string.IsNullOrEmpty(file.ContentHash)
        ? new EntityTagHeaderValue($"\"{id:N}-{file.LengthInBytes}\"", isWeak: true)
        : new EntityTagHeaderValue($"\"{file.ContentHash}\"");

    Response.Headers.CacheControl = "private, max-age=86400, must-revalidate";

    return File(
        fileStream: file.Stream,
        contentType: file.ContentType,
        fileDownloadName: asAttachment ? file.FileName : null,
        lastModified: null,
        entityTag: etag,
        enableRangeProcessing: true);
}
```

Three things still live in the controller, on purpose:

1. `Cache-Control` — the framework doesn't set it, and it's not file-specific
   in shape, so a small `Use(...)` middleware (like the one in §3) can move
   it out later if you want.
2. The strong-vs-weak ETag choice — only the controller knows the row's
   content hash and id.
3. The inline-vs-attachment toggle — driven by the route, not by domain.

Everything else is now the framework.

---

## Net-net comparison

| Approach | Auth-aware | Soft-delete-aware | Strong ETag | Streaming | Storage portable | Range | LOC in controller |
|---|---|---|---|---|---|---|---|
| `UseStaticFiles` | grafted on | no | weak only | yes | no | yes | 0 |
| `File(...)` + `[ResponseCache]` | yes | yes | no | yes | yes | yes | ~3 |
| Auto-ETag middleware | yes | yes | yes (buffers!) | no | yes | broken | 0 in controller, lots elsewhere |
| `File(... etag ...)` (the one I should use) | yes | yes | yes | yes | yes | yes | ~10 |
| Hand-rolled (current code) | yes | yes | yes | yes | yes | yes | ~25 |

The "should use" row keeps every property of the hand-rolled version while
delegating the ETag/conditional-GET dance to the framework. It's the right
default.

---

## Refactor offered

I can flip the current `FilesController.ServeAsync` to use the
`File(... entityTag …)` overload immediately. The behaviour is identical
from the wire's point of view (same headers, same status codes); the
controller is shorter and the conditional-GET logic moves into ASP.NET's
own well-tested code path.

If you want that change, say the word and I'll apply it.
