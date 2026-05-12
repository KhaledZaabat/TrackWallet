# Implementation Plan: Cookie-Based Auth Refactor

## Overview

This plan converts TrackWallet's authentication transport from JSON token bodies to HttpOnly cookies, adds server-side silent refresh, hashes refresh tokens, introduces session families with reuse detection, enforces a sliding-but-bounded lifetime, and wires ASP.NET Core anti-forgery into the pipeline — all while keeping every `[Authorize]` controller, filter, `IUserContext`, and `IFamilyContext` contract untouched (Requirement 16).

The tasks follow the component layout and request-pipeline ordering defined in `design.md` §Architecture, §Components and Interfaces, §Data Models, §Request Pipeline Ordering, §Configuration / Options Classes, §CSRF Integration Strategy, §Concurrency Strategy, §Logout Flow, §Security Considerations, §Performance Considerations, §Error Handling Matrix, §Backward Compatibility and Migration, §Correctness Properties, and §Testing Strategy. Each leaf task references its covering acceptance criteria from `requirements.md`, and property-based tests are annotated with the property number from `design.md` §Correctness Properties.

Ordering is layered: options and interfaces first (wave 0), infrastructure implementations next (waves 1–2), middleware and DI wiring (waves 2–3), pipeline assembly (wave 4), integration/convention tests (wave 5). The dependency graph at the end makes this explicit so safe parallelism is obvious.

Target language and stack: C# on .NET 10 (as already used by `Expense_Tracker.App`, `Expense_Tracker.Application`, `Expense_Tracker.Infrastructure`, `Expense_Tracker.Domain`, `Expense_Tracker.Contracts`). Tests use xUnit; property tests use FsCheck.Xunit; integration tests use `Microsoft.AspNetCore.Mvc.Testing` with `Testcontainers.PostgreSql` as stated in `design.md` §Testing Strategy.

Convert the feature design into a series of prompts for a code-generation LLM that will implement each step with incremental progress. Make sure that each prompt builds on the previous prompts, and ends with wiring things together. There should be no hanging or orphaned code that isn't integrated into a previous step. Focus ONLY on tasks that involve writing, modifying, or testing code.

## Tasks

- [ ] 1. Configuration options and DI binding
  - [x] 1.1 Create `AuthCookieOptions` in `Expense_Tracker.App/Auth/AuthCookieOptions.cs`
    - Add `SectionName = "AuthCookies"`, required `AccessCookieName`, `RefreshCookieName`, `CsrfCookieName`, per-cookie `SameSiteMode` properties (default `Strict`), required `AccessPath`, `RefreshPath` (default `/api/identity`), `CsrfPath`, optional `Domain`, and `AllowInsecureInDevelopment`
    - Decorate with `[Required]`/`[Range]` data annotations as shown in `design.md` §Configuration
    - _Requirements: 2.4, 3.4, 17.5, 22.2, 22.6, 22.7_
  - [x] 1.2 Create `CsrfOptions` in `Expense_Tracker.App/Auth/CsrfOptions.cs`
    - Add `SectionName = "Csrf"`, required `CookieName` (default `XSRF-TOKEN`), required `HeaderName` (default `X-XSRF-TOKEN`), `SameSite` (default `Strict`), and `ExemptPaths` pre-seeded with login, register, refresh, confirm-account (+ otp resend), and reset-password (+ otp send/verify)
    - _Requirements: 12.1, 12.5, 17.5, 22.6_
  - [x] 1.3 Extend `JwtSettings` in `Expense_Tracker.Application/Common/Settings/JwtSettings.cs`
    - Add `ClockSkewSeconds` (default 30, range 0–300), `SilentRefreshThresholdMinutes` (default 3, range 1–60), `AbsoluteSessionLifetimeDays` (default 180, range 1–3650), `RotationGraceSeconds` (default 10, range 1–120)
    - Add computed property `SilentRefreshThresholdAsTimeSpan`
    - _Requirements: 5.1, 11.2, 13.1, 17.4, 17.5_
  - [x] 1.4 Bind options in `Expense_Tracker.App/DependencyInjection.cs` and add matching `appsettings.json`/`appsettings.Development.json` sections
    - Add a new `AddCookieAuthConfiguration(IConfiguration)` extension that calls `services.AddOptions<AuthCookieOptions>().BindConfiguration(AuthCookieOptions.SectionName).ValidateDataAnnotations().Validate(o => o.AccessCookieName != o.RefreshCookieName, "Access and Refresh cookie names must differ.").ValidateOnStart()` and the equivalent for `CsrfOptions`
    - Wire the new extension into the existing `AddPresentation` chain alongside `AddJwtConfiguration`
    - Populate `appsettings.json` with `AuthCookies` and `Csrf` sections and extended `JwtSettings` fields; override `AuthCookies.AllowInsecureInDevelopment=true` in `appsettings.Development.json`
    - _Requirements: 2.4, 3.4, 12.1, 12.5, 13.1, 17.5, 22.6_
  - [ ]* 1.5 Write unit tests for options binding and validation
    - Assert `AuthCookieOptions` fails binding when `AccessCookieName == RefreshCookieName`
    - Assert `CsrfOptions` default `ExemptPaths` contains the required identity endpoints
    - Assert `JwtSettings.SilentRefreshThresholdAsTimeSpan` matches `SilentRefreshThresholdMinutes`
    - _Requirements: 2.4, 3.4, 12.5, 13.1_

- [ ] 2. Domain and infrastructure data model changes
  - [~] 2.1 Update `RefreshToken` entity in `Expense_Tracker.Infrastructure/Idenitity/RefreshToken.cs`
    - Add `byte[] TokenHash`, `Guid SessionFamilyId`, `DateTimeOffset OriginalIssuedAt`, `Guid? ReplacedByTokenId`
    - Remove the existing `public string Token { get; private set; }` accessor and any raw-token constructor parameters — the entity SHALL have no way to return the raw value from persistence
    - Update/replace `Create`/`Revoke`/`MarkReplacedBy` factories to match the new shape
    - _Requirements: 9.1, 9.4, 11.2, 18.5_
  - [~] 2.2 Update `RefreshTokenConfiguration` in `Expense_Tracker.Infrastructure/Data/Configurations/RefreshTokenConfiguration.cs`
    - Map `TokenHash` as `bytea(32)`, required; `SessionFamilyId` / `ReplacedByTokenId` as `uuid`; `OriginalIssuedAt` with `timestamptz`
    - Drop the old `HasIndex(x => x.Token).IsUnique()`
    - Add `HasIndex(x => x.TokenHash).IsUnique()`, `HasIndex(x => new { x.UserId, x.DeviceId })`, `HasIndex(x => new { x.SessionFamilyId, x.DeviceId })`, and `HasIndex(x => new { x.ExpiresAt, x.RevokedAt })`
    - _Requirements: 7.3, 9.2, 9.5, 10.2, 21.1_
  - [~] 2.3 Write the EF migration `CookieAuth_RefreshTokenRotation` in `Expense_Tracker.Infrastructure/Migrations`
    - `ALTER TABLE "RefreshTokens"` to add `TokenHash`, `SessionFamilyId`, `OriginalIssuedAt`, `ReplacedByTokenId` as nullable
    - `UPDATE "RefreshTokens" SET "RevokedAt" = now() WHERE "RevokedAt" IS NULL` to forcibly revoke every active row (design.md §EF migration plan step 5)
    - Backfill `TokenHash`, `SessionFamilyId`, `OriginalIssuedAt` on surviving rows so they satisfy `NOT NULL` without preserving usable token state
    - `DROP INDEX "IX_RefreshTokens_Token"`, `ALTER TABLE ... DROP COLUMN "Token"`, `ALTER COLUMN ... SET NOT NULL` for the three new mandatory columns
    - `CREATE UNIQUE INDEX "IX_RefreshTokens_TokenHash"` and `CREATE INDEX "IX_RefreshTokens_SessionFamilyId_DeviceId"`
    - _Requirements: 9.5, 18.5, 20.1, 20.2_
  - [ ]* 2.4 Write an integration test that applies the migration on a seeded `Token`-column snapshot and asserts the post-migration schema, unique `TokenHash` index, and that all previously-active rows end up revoked
    - _Requirements: 9.5, 20.1, 20.2_

- [ ] 3. Extend `IRefreshTokenService` abstraction (Application layer)
  - [~] 3.1 Extend `IRefreshTokenService` in `Expense_Tracker.Application/Interfaces/IRefreshTokenService.cs`
    - Add `Task<ErrorOr<Success>> AddNewSessionAsync(Guid userId, string rawToken, string deviceId, Guid sessionFamilyId, DateTimeOffset originalIssuedAt, CancellationToken ct = default)`
    - Add `Task<ErrorOr<RotationSuccess>> RotateAsync(string rawIncomingToken, string deviceId, CancellationToken ct)`
    - Add `Task<ErrorOr<Success>> RevokeAllSessionsForUserAsync(Guid userId, CancellationToken ct = default)`
    - Define `readonly record struct RotationSuccess(string NewRawToken, DateTimeOffset NewRefreshExpiresAt, Guid SessionFamilyId, DateTimeOffset OriginalIssuedAt, AuthenticatedUser User, FamilyContextDto? Family)` in the same namespace
    - _Requirements: 8.1, 8.2, 10.1, 10.2, 11.1, 17.4, 21.4_

- [ ] 4. `RefreshTokenService` implementation (Infrastructure layer)
  - [~] 4.1 Add helpers and non-rotating methods to `Expense_Tracker.Infrastructure/Idenitity/RefreshTokenService.cs`
    - Add a private `static byte[] Sha256(string raw)` helper using `SHA256.HashData(Encoding.UTF8.GetBytes(raw))`
    - Rewrite `RevokeActiveTokensAsync(userId, deviceId, ct)` to operate on the new schema (no raw token compare)
    - Implement `AddNewSessionAsync` that persists a `RefreshToken` row with `TokenHash = Sha256(raw)`, the supplied `SessionFamilyId`/`OriginalIssuedAt`, and `ExpiresAt = now + RefreshTokenExpirationDays`
    - Implement `RevokeAllSessionsForUserAsync(userId, ct)` by `UPDATE "RefreshTokens" SET "RevokedAt" = now() WHERE "UserId" = @u AND "RevokedAt" IS NULL`
    - _Requirements: 9.1, 9.2, 9.3, 14.1, 18.5, 21.3, 21.4_
  - [~] 4.2 Implement atomic `RotateAsync` in the same file
    - Open a transaction, run `SELECT ... FROM "RefreshTokens" WHERE "TokenHash" = @hash AND "DeviceId" = @deviceId FOR UPDATE` via `DbContext.Database.ExecuteSqlRaw` / `FromSqlInterpolated`
    - Guard with `CryptographicOperations.FixedTimeEquals(existing.TokenHash, incomingHash)` as defense-in-depth (design.md §Components — RefreshTokenService)
    - On no-row or expired row → return `Error.InvalidRefreshToken` / `Error.RefreshTokenExpired`
    - On `RevokedAt IS NOT NULL` → `UPDATE "RefreshTokens" SET "RevokedAt" = now() WHERE "SessionFamilyId" = @fam AND "DeviceId" = @d AND "RevokedAt" IS NULL` and return `Error.ReuseDetected`; log a security event containing `UserId`, `DeviceId`, `SessionFamilyId`, IP, UA with no token values
    - Enforce `now - OriginalIssuedAt > AbsoluteSessionLifetimeDays` absolute-lifetime rejection
    - On success, `UPDATE` the old row to set `RevokedAt` + `ReplacedByTokenId`, `INSERT` a new row with the same `SessionFamilyId` / `OriginalIssuedAt`, sliding `ExpiresAt = now + RefreshTokenExpirationDays`, commit, return `RotationSuccess` carrying the raw new token and user/family context
    - _Requirements: 7.2, 7.3, 8.1, 8.2, 9.2, 9.3, 10.1, 10.2, 10.4, 11.1, 11.2, 11.3, 11.4, 18.2, 18.4, 21.1, 21.2_
  - [ ]* 4.3 Write unit tests for `RotateAsync` success path against the in-memory EF provider
    - Assert exactly one `UPDATE` + one `INSERT` per rotation, `SessionFamilyId` + `OriginalIssuedAt` preserved, new `ExpiresAt` slides, `ReplacedByTokenId` set
    - _Requirements: 7.2, 9.3, 11.1_
  - [ ]* 4.4 Write unit test for reuse detection
    - Rotate once, replay the original raw token, assert `Error.ReuseDetected` and that every `(SessionFamilyId, DeviceId)` row transitions to `RevokedAt != null`
    - _Requirements: 10.1, 10.2_
  - [ ]* 4.5 Write unit test for absolute-lifetime rejection
    - Backdate `OriginalIssuedAt` to before `now - AbsoluteSessionLifetimeDays`, call `RotateAsync`, assert rejection without inserting a new row
    - _Requirements: 11.2, 11.3_
  - [ ]* 4.6 Write unit test asserting `CryptographicOperations.FixedTimeEquals` is actually called (source/analyzer assertion or delegate injection)
    - _Requirements: 18.2_
  - [ ]* 4.7 Write property-based test for **Property 15: Rotation DB budget** (exactly one indexed `SELECT ... FOR UPDATE` + one transactional write per rotation)
    - Use FsCheck.Xunit with an in-memory EF provider and a `SaveChanges` / command counter
    - _Requirements: 7.2, 7.3, 9.2, 9.3, 19.2_
    - _Properties: 15_
  - [ ]* 4.8 Write property-based test for **Property 17: Rotation family invariants** (`SessionFamilyId` preserved, `OriginalIssuedAt` preserved, `ReplacedByTokenId` chain, sliding `ExpiresAt`, device scoping, absolute-lifetime enforcement)
    - Generate rotation chains of random length and assert every invariant across every link
    - _Requirements: 9.4, 11.1, 11.2, 21.1_
    - _Properties: 17_
  - [ ]* 4.9 Write property-based test for **Property 18: Hash-only storage**
    - Generate raw refresh tokens, assert `TokenHash = SHA256(UTF8(raw))` and that no persisted column contains the raw value or any reversible encoding
    - _Requirements: 9.1, 18.5_
    - _Properties: 18_
  - [ ]* 4.10 Write property-based test for **Property 19: Reuse detection revokes the entire family for the device**
    - Generate a chain `r0..rk`, replay any `r_j` (j<k), assert every `(SessionFamilyId, DeviceId)` row transitions to revoked and sibling devices are unaffected
    - _Requirements: 10.1, 10.2, 21.2_
    - _Properties: 19_
  - [ ]* 4.11 Write property-based test for **Property 16: Concurrency-safe rotation** using `Testcontainers.PostgreSql` (in-memory EF does not honor `SELECT ... FOR UPDATE`)
    - Launch N concurrent `RotateAsync` calls for the same `(rawIncomingToken, deviceId)`, assert exactly one successor row and no caller gets `ReuseDetected` purely from the race
    - _Requirements: 8.1, 8.2, 8.3, 8.4_
    - _Properties: 16_
  - [ ]* 4.12 Write property-based test for **Property 24: Logout revokes only current-device tokens**
    - Seed multiple devices, call `RevokeActiveTokensAsync(userId, deviceId)`, assert only rows with that `DeviceId` transition to revoked
    - _Requirements: 14.1, 21.3_
    - _Properties: 24_

- [ ] 5. Extend `ITokenProvider` abstraction (Application layer)
  - [~] 5.1 Add `GenerateAccessTokenOnlyAsync` and supporting types to `Expense_Tracker.Application/Interfaces/ITokenProvider.cs`
    - Define `public readonly record struct AccessTokenResult(string Token, DateTimeOffset ExpiresAt)`
    - Add `Task<AccessTokenResult> GenerateAccessTokenOnlyAsync(AuthenticatedUser user, FamilyContextDto? family, CancellationToken ct)`
    - _Requirements: 5.3, 17.4, 19.2_

- [ ] 6. `TokenProvider` changes (Infrastructure layer)
  - [~] 6.1 Update the mint path in `Expense_Tracker.Infrastructure/Idenitity/TokenProvider.cs`
    - Replace the existing refresh-token generator with `GenerateOpaqueRefreshToken` that returns `Base64UrlEncode(RandomNumberGenerator.GetBytes(32))`
    - In `GenerateJwtTokenWithFamilyAsync` (and any sibling login path), generate a new `Guid.CreateVersion7()` `SessionFamilyId`, call `IRefreshTokenService.RevokeActiveTokensAsync(userId, deviceId)` then `AddNewSessionAsync(userId, rawRefresh, deviceId, sessionFamilyId, originalIssuedAt: clock.UtcNow)`
    - Keep the existing `AuthDto` shape; the raw refresh remains internal and never serialized to HTTP bodies (Requirement 1)
    - _Requirements: 1.1, 1.2, 9.3, 9.4, 11.1, 18.3, 18.5_
  - [~] 6.2 Add `GenerateAccessTokenOnlyAsync` and update `GetPrincipalFromExpiredToken` in the same file
    - Implement `GenerateAccessTokenOnlyAsync` by building the claims via `BuildClaims(user, familyContext)` and minting a JWT with `ExpiresAt = clock.UtcNow.AddMinutes(JwtSettings.AccessTokenExpirationMinutes)` — no refresh-token side effect
    - Update `GetPrincipalFromExpiredToken` so its `TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(JwtSettings.ClockSkewSeconds)`
    - _Requirements: 5.3, 13.3, 16.2, 16.3, 19.1_
  - [ ]* 6.3 Write unit tests for `TokenProvider` helpers: `GenerateAccessTokenOnlyAsync` produces a valid JWT with the expected `ExpiresAt`, and `GetPrincipalFromExpiredToken` honors the configured skew
    - _Requirements: 5.3, 13.3, 16.2_
  - [ ]* 6.4 Write property-based test for **Property 4: Opaque refresh token shape**
    - Generate many raw refresh values, assert base64url-decoded length ≥ 32 and that the value is NOT a three-segment JWT
    - _Requirements: 3.5, 18.3_
    - _Properties: 4_
  - [ ]* 6.5 Write property-based test for **Property 7: ClaimsPrincipal shape preserved**
    - Generate random `AuthenticatedUser` + optional `FamilyContextDto`, mint and parse the token, assert the claim type set matches the canonical set from `design.md` §Correctness Properties / Property 7 (sub, jti, `CustomClaimTypes.UserId`, email, `CustomClaimTypes.Email`, `ClaimTypes.Name`, `CustomClaimTypes.UserName`, family claims when present)
    - _Requirements: 4.3, 16.2, 16.3_
    - _Properties: 7_

- [ ] 7. `JwtBearerOptionsConfigurator` (Infrastructure layer)
  - [~] 7.1 Update `Expense_Tracker.Infrastructure/Idenitity/JwtBearerOptionsConfigurator.cs`
    - Inject `IOptions<AuthCookieOptions>`
    - Set `TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(JwtSettings.ClockSkewSeconds)`
    - Install `Events.OnMessageReceived` that reads the Access Token Cookie; when the cookie is missing or empty call `ctxEvt.NoResult()` so the `Authorization: Bearer` header is NEVER used as a fallback
    - _Requirements: 4.1, 4.2, 4.4, 13.1, 20.5_
  - [ ]* 7.2 Write unit tests covering cookie-present, cookie-missing (with and without an `Authorization` header), and clock-skew configuration
    - _Requirements: 4.1, 4.2, 4.4, 13.1_
  - [ ]* 7.3 Write property-based test for **Property 6: JWT bearer token is resolved exclusively from the Access Token Cookie**
    - Generate arbitrary `Cookie` + `Authorization` header combinations, assert token observed by validation equals the cookie value whenever present, and is unset (causing 401 on `[Authorize]`) when the cookie is missing
    - _Requirements: 4.1, 4.2, 15.1_
    - _Properties: 6_

- [ ] 8. `IAuthCookieWriter` and `AuthCookieWriter` (App layer)
  - [~] 8.1 Define `IAuthCookieWriter` and `AuthCookieDescriptor` in `Expense_Tracker.App/Auth/IAuthCookieWriter.cs`
    - `WriteAccessCookie(HttpContext, string, DateTimeOffset)`, `WriteRefreshCookie(HttpContext, string, DateTimeOffset)`, `IssueCsrfCookie(HttpContext)`, `RefreshCsrfCookie(HttpContext)`, `ClearAuthCookies(HttpContext)`, `IReadOnlyList<AuthCookieDescriptor> GetRegisteredDescriptors()`
    - `public sealed record AuthCookieDescriptor(string Name, bool HttpOnly, bool Secure, SameSiteMode SameSite, string Path, string? Domain)`
    - _Requirements: 17.1, 22.2_
  - [~] 8.2 Implement `AuthCookieWriter` in `Expense_Tracker.App/Auth/AuthCookieWriter.cs` as `IScopedService`
    - Construct with `IOptionsMonitor<AuthCookieOptions>`, `IOptionsMonitor<CsrfOptions>`, `IWebHostEnvironment`, `IAntiforgery`
    - Build `CookieOptions` enforcing `HttpOnly=true` (access, refresh) and `HttpOnly=false` (CSRF), `Secure=true` always with the Development opt-out (`AllowInsecureInDevelopment == true && env.IsDevelopment()`), explicit `SameSite`/`Path`/`Domain`/`Expires`, distinct cookie names, access expires = access-token `exp`, refresh expires = rotation-extended expiry
    - Implement `IssueCsrfCookie`/`RefreshCsrfCookie` by calling `IAntiforgery.GetAndStoreTokens(ctx)` then re-asserting the `CsrfOptions`-owned `CookieOptions` via `Response.Cookies.Append`
    - Implement `ClearAuthCookies` so it emits clear headers with the exact same `Name`/`Path`/`Domain`/`Secure`/`SameSite`/`HttpOnly` used on write
    - Implement `GetRegisteredDescriptors` to return the (access, refresh, CSRF) descriptors for the startup validator
    - _Requirements: 2.1–2.5, 3.1–3.5, 12.2, 12.6, 14.2, 17.1, 22.2–22.9_
  - [ ]* 8.3 Write unit tests for `AuthCookieWriter`
    - Access/refresh/CSRF `Set-Cookie` shape; `AllowInsecureInDevelopment` path; `ClearAuthCookies` emits three expired cookies with matching attributes; `RefreshCsrfCookie` results in exactly one CSRF `Set-Cookie`
    - _Requirements: 2.3, 2.5, 3.3, 14.2, 22.3–22.5, 22.9_
  - [ ]* 8.4 Write property-based test for **Property 2: Auth cookie attribute invariants**
    - Generate `AuthCookieOptions` + environment + flag combinations, assert `HttpOnly`, `Secure`, `SameSite`, `Path`, `Domain`, `Expires`/`Max-Age` invariants per cookie
    - _Requirements: 2.3, 2.5, 3.3, 12.6, 22.3, 22.4, 22.5, 22.6, 22.7_
    - _Properties: 2_
  - [ ]* 8.5 Write property-based test for **Property 3: Cookie set/clear attribute parity**
    - For every registered descriptor, assert the `Set-Cookie` emitted on clear equals the one emitted on set (same `Name`, `Path`, `Domain`, `Secure`, `SameSite`, `HttpOnly`) with `Max-Age=0`
    - _Requirements: 14.2, 22.9_
    - _Properties: 3_

- [ ] 9. `EndpointAuthInspector` (App layer)
  - [~] 9.1 Implement `EndpointAuthInspector` in `Expense_Tracker.App/Auth/EndpointAuthInspector.cs`
    - Public static `bool RequiresAuthorization(HttpContext ctx)` that returns `false` when endpoint is null or has `IAllowAnonymous`, and `true` iff `IAuthorizeData` metadata is present
    - _Requirements: 6.1, 6.2, 6.3, 6.5_
  - [ ]* 9.2 Write unit truth-table tests for the four combinations of `IAuthorizeData` × `IAllowAnonymous` plus the null-endpoint case
    - _Requirements: 6.1, 6.2, 6.3_
  - [ ]* 9.3 Write property-based test for **Property 8: Silent refresh endpoint gating**
    - Generate synthetic endpoints with random metadata combinations, assert the inspector's decision matches the specification
    - _Requirements: 6.1, 6.2, 6.3, 6.4_
    - _Properties: 8_

- [ ] 10. `SilentRefreshMiddleware` (App layer)
  - [~] 10.1 Implement `SilentRefreshMiddleware` in `Expense_Tracker.App/Auth/SilentRefreshMiddleware.cs`
    - Inject `RequestDelegate next`, `IOptionsMonitor<JwtSettings>`, `IOptionsMonitor<AuthCookieOptions>`, `IMemoryCache` (existing singleton), `ILogger<SilentRefreshMiddleware>`
    - Short-circuit when `ctx.Items["__trackwallet_silent_refresh_ran"]` or `ctx.Items["AuthLogoutInProgress"]` is set
    - Gate on `EndpointAuthInspector.RequiresAuthorization(ctx)`
    - Compute remaining access-token lifetime in memory from `JwtRegisteredClaimNames.Exp`, adjusted by `JwtSettings.ClockSkewSeconds`; above the `SilentRefreshThresholdAsTimeSpan` → pass through WITHOUT resolving `IRefreshTokenService` / `ITokenProvider` / `IAuthCookieWriter`
    - Within threshold: set the per-request marker, lazily resolve services from `ctx.RequestServices`, read refresh cookie + device id, check `IMemoryCache` for `rot:{sha256Hex(rawRefresh)}` grace hit and on hit re-write cookies and swap principal without DB
    - On cache miss call `IRefreshTokenService.RotateAsync(...)`, on success mint access via `ITokenProvider.GenerateAccessTokenOnlyAsync`, cache the `RotationResult` for `JwtSettings.RotationGraceSeconds`, write both cookies + CSRF via `IAuthCookieWriter`, replace `HttpContext.User` with the new principal
    - On any rotation error (`Invalid`, `Expired`, `ReuseDetected`, absolute-lifetime exceeded) call `IAuthCookieWriter.ClearAuthCookies(ctx)` and continue to `UseAuthorization` so the default 401 fires
    - NEVER log raw token values; log only `UserId`, `DeviceId`, `SessionFamilyId`, IP, UA
    - _Requirements: 5.1–5.6, 6.4, 7.1, 7.2, 7.4, 8.3, 8.4, 10.3, 11.3, 13.2, 14.4, 16, 18.4, 19.1, 19.3_
  - [ ]* 10.2 Write unit test: above-threshold request invokes `_next` with zero DI resolutions of `IRefreshTokenService`/`ITokenProvider` and zero `AppDbContext.RefreshTokens` queries
    - _Requirements: 5.2, 7.1, 19.1, 19.3_
  - [ ]* 10.3 Write unit test: re-invocation of the middleware with the per-request marker set is a no-op
    - _Requirements: 5.6_
  - [ ]* 10.4 Write unit test: `ctx.Items["AuthLogoutInProgress"]` set → middleware writes no cookies and performs no rotation
    - _Requirements: 14.4_
  - [ ]* 10.5 Write unit test: grace cache hit → no DB calls, winning tokens written to response, principal swapped
    - _Requirements: 7.4, 8.3, 8.4_
  - [ ]* 10.6 Write unit test: rotation success → cookies written, principal replaced, `_next` sees the new identity
    - _Requirements: 5.3, 5.4, 15.4_
  - [ ]* 10.7 Write unit test: rotation error → both auth cookies cleared via `IAuthCookieWriter.ClearAuthCookies` and pipeline falls through to 401
    - _Requirements: 5.5, 10.3, 11.3_
  - [ ]* 10.8 Write property-based test for **Property 9: Silent refresh threshold decision honors clock skew**
    - Generate `(exp, now, ClockSkewSeconds, SilentRefreshThresholdMinutes)` tuples, assert decision matches `(exp - now + skew) <= threshold`
    - _Requirements: 5.1, 5.2, 13.1, 13.2, 13.3, 19.1_
    - _Properties: 9_
  - [ ]* 10.9 Write property-based test for **Property 10: Silent refresh success effects**
    - Generate within-threshold requests with valid refresh cookies, assert fresh access + refresh `Set-Cookie` with correct `Max-Age`, principal replaced, inner endpoint body unchanged
    - _Requirements: 5.3, 5.4, 11.4, 15.4_
    - _Properties: 10_
  - [ ]* 10.10 Write property-based test for **Property 11: Silent refresh failure clears cookies and falls through to 401**
    - Generate invalid/expired/reuse/absolute-exceeded inputs, assert `ClearAuthCookies` is called and pipeline produces 401
    - _Requirements: 5.5, 10.3, 11.3_
    - _Properties: 11_
  - [ ]* 10.11 Write property-based test for **Property 12: Silent refresh is idempotent per request**
    - Invoke the middleware twice on the same `HttpContext`, assert the second invocation produces the same observable response as the first
    - _Requirements: 5.6_
    - _Properties: 12_
  - [ ]* 10.12 Write property-based test for **Property 13: Logout skip marker prevents re-issue**
    - Pre-set `ctx.Items["AuthLogoutInProgress"]`, assert no auth `Set-Cookie` emitted by the middleware
    - _Requirements: 14.4_
    - _Properties: 13_
  - [ ]* 10.13 Write property-based test for **Property 14: Lazy DI / no DB on non-rotating path**
    - Above-threshold requests: assert middleware never touches `ctx.RequestServices.GetService<IRefreshTokenService>()` nor `AppDbContext.RefreshTokens`
    - _Requirements: 7.1, 7.4, 19.1, 19.3_
    - _Properties: 14_

- [ ] 11. `CsrfValidationMiddleware` and antiforgery registration (App layer)
  - [~] 11.1 Implement `CsrfValidationMiddleware` in `Expense_Tracker.App/Auth/CsrfValidationMiddleware.cs`
    - Inject `RequestDelegate next`, `IAntiforgery antiforgery`, `IOptionsMonitor<CsrfOptions> csrfOpts`
    - Short-circuit when the path starts with any `CsrfOptions.ExemptPaths` entry
    - Validate only on unsafe methods (`POST`/`PUT`/`PATCH`/`DELETE`) AND when `EndpointAuthInspector.RequiresAuthorization(ctx)` is `true`
    - On `AntiforgeryValidationException` short-circuit with `403 Forbidden` and emit no additional cookies
    - _Requirements: 12.3, 12.4, 12.5_
  - [~] 11.2 Register `AddAntiforgery` in `Expense_Tracker.App/DependencyInjection.cs`
    - Bind `Cookie.Name` / `HeaderName` from `CsrfOptions`, `Cookie.HttpOnly = false`, `Cookie.SameSite = csrf.SameSite`, `Cookie.SecurePolicy = CookieSecurePolicy.Always`
    - _Requirements: 12.1, 22.4, 22.5, 22.6_
  - [ ]* 11.3 Write unit test covering the decision truth table (method × requires-auth × exempt-path)
    - _Requirements: 12.3, 12.5_
  - [ ]* 11.4 Write unit test: `IAntiforgery.ValidateRequestAsync` throws → middleware short-circuits with 403 and emits no `Set-Cookie`
    - _Requirements: 12.4_
  - [ ]* 11.5 Write property-based test for **Property 21: CSRF validation decision**
    - Generate `(method, path, endpoint-metadata)` tuples, assert validation runs iff method ∈ {POST, PUT, PATCH, DELETE}, `RequiresAuthorization` true, and the path is not exempt
    - _Requirements: 12.3, 12.5_
    - _Properties: 21_
  - [ ]* 11.6 Write property-based test for **Property 22: CSRF validation failure short-circuits without side effects**
    - Generate failing requests, assert response is 403, no new auth `Set-Cookie` was written, and no `AppDbContext` write was attributable to a rotation after the failure
    - _Requirements: 12.4_
    - _Properties: 22_
  - [ ]* 11.7 Write property-based test for **Property 23: CSRF cookie refreshed on authenticated responses**
    - Generate successful login / refresh / within-threshold silent-refresh flows, assert the response contains a `Set-Cookie` for `CsrfOptions.CookieName` issued through `IAuthCookieWriter.RefreshCsrfCookie`
    - _Requirements: 12.2_
    - _Properties: 23_

- [ ] 12. `AuthCookieStartupValidator` (App layer)
  - [~] 12.1 Implement `AuthCookieStartupValidator : IHostedService` in `Expense_Tracker.App/Auth/AuthCookieStartupValidator.cs`
    - Inject `IAuthCookieWriter`, `IWebHostEnvironment`
    - In `StartAsync`, skip when `env.IsDevelopment()`; otherwise call `GetRegisteredDescriptors()` and assert `HttpOnly=true` for access/refresh, `HttpOnly=false` for CSRF, `Secure=true` for all, non-null `Path`, explicit `SameSite`; throw `InvalidOperationException` on any mismatch so the host fails fast
    - _Requirements: 18.1, 22.3, 22.4, 22.5, 22.7, 22.8_
  - [~] 12.2 Register `AuthCookieStartupValidator` as `IHostedService` in `Expense_Tracker.App/DependencyInjection.cs`
    - Hook into the existing `AddPresentation` pipeline
    - _Requirements: 17.5, 22.8_
  - [ ]* 12.3 Write unit test: production environment with `Secure=false` fails startup; Development with `AllowInsecureInDevelopment=true` does not
    - _Requirements: 18.1, 22.8_
  - [ ]* 12.4 Write property-based test for **Property 25: Production startup rejects insecure auth cookies**
    - Generate descriptor sets with random `HttpOnly`/`Secure`/`SameSite`/`Path` values in non-Development environments, assert startup fails iff any invariant from `design.md` §Configuration — startup validator is violated
    - _Requirements: 18.1, 22.8_
    - _Properties: 25_

- [ ] 13. Serilog `AuthTokenScrubber`
  - [~] 13.1 Implement `AuthTokenScrubber` in `Expense_Tracker.App/Logging/AuthTokenScrubber.cs`
    - Serilog `ILogEventEnricher` / filter that removes or masks properties named `TokenHash`, `accessToken`, `refreshToken`, `xsrf`, `Cookie`, `Set-Cookie`, and any sub-property of the cookie option set
    - _Requirements: 10.4, 18.4_
  - [~] 13.2 Wire `AuthTokenScrubber` into the Serilog pipeline in `Expense_Tracker.App/Program.cs`
    - Add `.Enrich.With<AuthTokenScrubber>()` / `.Filter.With(...)` next to existing Serilog configuration
    - _Requirements: 10.4, 18.4_
  - [ ]* 13.3 Write unit test: given a log event containing each of the forbidden fields, assert the scrubber strips or masks every one
    - _Requirements: 10.4, 18.4_
  - [ ]* 13.4 Write property-based test for **Property 20: No token values in logs**
    - Drive login / refresh / silent-refresh success / reuse-detection / logout through a `TestSink` logger, assert no emitted record contains any raw or hashed token value or cookie header
    - _Requirements: 10.4, 18.4_
    - _Properties: 20_

- [ ] 14. Application-layer DTOs and command handlers
  - [~] 14.1 Update `AuthResponse` in `Expense_Tracker.Application/.../AuthResponse.cs`
    - Keep only `UserId`, `Email`, `FullName`, `ProfileImageUrl`, `Families`
    - Remove `JwtToken` and `RefreshToken` fields and any types (`TokenResponse`) referenced solely by the HTTP response body
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 20.3_
  - [~] 14.2 Remove the `RefreshToken` field from `RefreshTokenRequest` in `Expense_Tracker.Contracts/Requests/Identity/RefreshTokenRequest.cs`
    - Keep `DeviceId` and `FcmToken` only
    - _Requirements: 15.1, 15.2_
  - [~] 14.3 Update `LoginCommand` / `LoginCommandHandler` in `Expense_Tracker.Application/Features/Login`
    - Command returns `ErrorOr<AuthResponse>` where `AuthResponse` is the cookie-less contract from 14.1
    - Handler consumes extended `ITokenProvider` / `IRefreshTokenService` and exposes the raw access + refresh tokens to the controller via an internal per-request result type so the controller can write cookies via `IAuthCookieWriter`
    - _Requirements: 1.1, 1.3, 2.1, 3.1, 9.3, 9.4, 20.3_
  - [~] 14.4 Update `RefreshTokenCommand` / `RefreshTokenCommandHandler` in `Expense_Tracker.Application/Features/Refresh`
    - Drop the `RefreshToken` parameter from the command; accept the raw refresh value from a controller-supplied argument that is always sourced from the cookie
    - Return the cookie-less `AuthResponse`
    - _Requirements: 15.1, 15.2, 15.4, 15.5_
  - [~] 14.5 Update `LogoutCommand` / `LogoutCommandHandler` in `Expense_Tracker.Application/Features/Identity/Commands/Logout`
    - Ensure the handler calls `IRefreshTokenService.RevokeActiveTokensAsync(userId, deviceId)` only (device-scoped), without cookie side effects (the controller owns cookie clearing)
    - _Requirements: 14.1, 21.3_
  - [ ]* 14.6 Write property-based test for **Property 1: Auth-endpoint response bodies contain no token values**
    - Generate random `AuthResponse` instances and successful login/refresh return shapes, serialize with the production `JsonSerializerOptions`, assert no access or refresh token value appears in the JSON
    - _Requirements: 1.1, 1.2_
    - _Properties: 1_

- [ ] 15. `IdentityController` changes (App layer)
  - [~] 15.1 Update `Expense_Tracker.App/Controllers/IdentityController.cs` Login / Refresh / Logout actions
    - Inject `IAuthCookieWriter`
    - `Login`: after the `LoginCommand` succeeds, call `writer.WriteAccessCookie`, `writer.WriteRefreshCookie`, `writer.IssueCsrfCookie`; return the cookie-less `AuthResponse`
    - `RefreshToken`: read the raw refresh value from `HttpContext.Request.Cookies[AuthCookieOptions.RefreshCookieName]`; return `401 Unauthorized` when the cookie is missing; pass the raw value into `RefreshTokenCommand`; on success call `WriteAccessCookie`/`WriteRefreshCookie`/`RefreshCsrfCookie`
    - `Logout`: set `HttpContext.Items["AuthLogoutInProgress"] = true` BEFORE dispatching `LogoutCommand`; on success call `writer.ClearAuthCookies(HttpContext)`; body stays empty
    - Do NOT introduce any direct `Response.Cookies.Append`/`Delete` calls for auth cookie names (Requirement 22.10)
    - _Requirements: 1.1, 1.2, 2.1, 2.2, 3.1, 3.2, 12.2, 14.1–14.4, 15.1, 15.3, 15.4, 15.5, 22.10_
  - [ ]* 15.2 Write unit test: controller invokes `IAuthCookieWriter` for login success, refresh success (reading the raw value from the cookie), and logout (setting the skip marker then clearing cookies)
    - _Requirements: 2.1, 2.2, 3.1, 3.2, 14.2, 14.4, 15.1, 15.3_
  - [ ]* 15.3 Write property-based test for **Property 5: Login and refresh both issue both auth cookies**
    - Drive arbitrary successful login and refresh executions through a `WebApplicationFactory`, assert the response contains `Set-Cookie` for both the configured access and refresh names
    - _Requirements: 2.1, 2.2, 3.1, 3.2, 20.5_
    - _Properties: 5_

- [ ] 16. `Program.cs` pipeline wiring
  - [~] 16.1 Update the request pipeline in `Expense_Tracker.App/Program.cs`
    - Replace the current `UseRouting → UseAuthentication → UseAuthorization → MapControllers` stanza with `UseRouting → UseCors("AllowFrontend") → UseAuthentication → UseMiddleware<SilentRefreshMiddleware>() → UseMiddleware<CsrfValidationMiddleware>() → UseAuthorization → MapControllers`
    - Re-enable the existing `AllowFrontend` CORS policy with explicit origins (dev + prod from config) plus `AllowCredentials()`; ensure `AllowAnyOrigin()` is never combined with credentials
    - _Requirements: 5, 6, 12, 17.2, 18.6, 20.4_

- [~] 17. Checkpoint
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 18. Integration tests (WebApplicationFactory + Testcontainers.PostgreSql)
  - [~] 18.1 Create the `Expense_Tracker.IntegrationTests` xUnit project with `Microsoft.AspNetCore.Mvc.Testing`, `Testcontainers.PostgreSql`, `FsCheck.Xunit`, reference `Expense_Tracker.App`
    - Add a `CookieAuthWebApplicationFactory` fixture that applies the new EF migration on a fresh Postgres container, registers test-only `AuthCookies` with `AllowInsecureInDevelopment=true`
    - _Requirements: 5, 6, 8, 9, 10, 11, 12, 14, 15, 16_
  - [ ]* 18.2 Integration test: `POST /api/identity/login` returns 200, emits `Set-Cookie` for access, refresh, and CSRF names, and the response body contains no token values
    - _Requirements: 1.1, 2.1, 3.1, 12.2_
    - _Properties: 1, 5_
  - [ ]* 18.3 Integration test: authenticated `GET` above the silent-refresh threshold does not touch `RefreshTokens` (via a command counter attached to `AppDbContext`)
    - _Requirements: 7.1, 19.1, 19.3_
    - _Properties: 14_
  - [ ]* 18.4 Integration test: authenticated `GET` within the threshold receives new access + refresh + CSRF `Set-Cookie` headers and remains authenticated transparently
    - _Requirements: 5.3, 5.4, 11.4, 12.2_
    - _Properties: 10, 23_
  - [ ]* 18.5 Integration test: expired access cookie + valid refresh cookie → silent refresh succeeds, controller executes, response carries rotated cookies
    - _Requirements: 5.3, 5.4, 15.4_
    - _Properties: 10_
  - [ ]* 18.6 Integration test: reuse attack — after a successful rotation, replay the original refresh cookie → 401, all cookies cleared, every `(SessionFamilyId, DeviceId)` row in the family is revoked, and the newest rotated cookie is also unusable
    - _Requirements: 10.1, 10.2, 10.3, 18.4_
    - _Properties: 19_
  - [ ]* 18.7 Integration test: `POST /api/identity/logout` clears all three cookies, revokes every active row for `(UserId, DeviceId)`, and leaves sibling-device rows untouched
    - _Requirements: 14.1, 14.2, 21.3_
    - _Properties: 24_
  - [ ]* 18.8 Integration test: CSRF exempt paths (`/api/identity/login`, `/api/identity/refresh`, `/api/identity/register`, password-reset and confirm-account endpoints) succeed without an `X-XSRF-TOKEN` header
    - _Requirements: 12.5_
  - [ ]* 18.9 Integration test: authenticated unsafe method without a valid `X-XSRF-TOKEN` → 403 Forbidden and no auth `Set-Cookie`
    - _Requirements: 12.3, 12.4_
    - _Properties: 22_
  - [ ]* 18.10 Integration test: cookie-only authentication — a request bearing only `Authorization: Bearer <valid-jwt>` on an `[Authorize]` endpoint returns 401; the same request with the access cookie set returns 2xx
    - _Requirements: 4.2, 4.4, 20.5_
    - _Properties: 6_

- [ ] 19. Smoke / convention tests
  - [ ]* 19.1 Reflection test: `AuthResponse` public property set equals `{UserId, Email, FullName, ProfileImageUrl, Families}`
    - _Requirements: 1.3, 20.3_
  - [ ]* 19.2 Reflection test: `RefreshTokenRequest` has no `RefreshToken` property
    - _Requirements: 15.2_
  - [ ]* 19.3 Assembly scan: no call to `Response.Cookies.Append` or `Response.Cookies.Delete` with any configured auth or CSRF cookie name exists outside `AuthCookieWriter`
    - _Requirements: 22.10_
  - [ ]* 19.4 Assembly scan: no HTTP-exposed method returns a `TokenResponse` in its response body (searching controller action return types + `ProducesResponseType` metadata)
    - _Requirements: 1.4_
  - [ ]* 19.5 Assembly placement assertions: `AuthCookieWriter`, `SilentRefreshMiddleware`, `EndpointAuthInspector`, `CsrfValidationMiddleware`, `AuthCookieOptions`, `CsrfOptions`, `AuthCookieStartupValidator` live in `Expense_Tracker.App`; `RefreshTokenService`, `TokenProvider`, `JwtBearerOptionsConfigurator` live in `Expense_Tracker.Infrastructure`; `ITokenProvider`, `IRefreshTokenService`, `AuthResponse` live in `Expense_Tracker.Application`
    - _Requirements: 17.1, 17.2, 17.3, 17.4, 17.5, 17.6_

- [~] 20. Final checkpoint
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 21. Documentation
  - [~] 21.1 Update `README.md` (and any deployment notes) to document the one-time session invalidation caveat from the migration and the required frontend changes
    - Note that the migration revokes every active session exactly once (Requirement 20.2)
    - Document that the browser frontend must call fetch/axios with `credentials: "include"` and echo `X-XSRF-TOKEN` on `POST`/`PUT`/`PATCH`/`DELETE`
    - State that the `Authorization: Bearer` header is no longer accepted (Requirement 20.5)
    - _Requirements: 20.2, 20.4, 20.5_

## Notes

- Tasks marked with `*` are optional test-related sub-tasks and can be skipped for a faster MVP; they are still included in the dependency graph for scheduling but the agent will not implement them unless explicitly requested.
- Every leaf task lists the requirements it satisfies. Property-based test sub-tasks additionally annotate the correctness property they exercise from `design.md` §Correctness Properties.
- Tasks 17 and 20 are checkpoints and are intentionally excluded from the dependency graph.
- Controllers outside `IdentityController` (`BudgetController`, `CategoriesController`, `DashboardController`, `FamiliesController`, `FamilyTransactionsController`, `FilesController`, `InvitationsController`, `NotificationPreferencesController`, `UserController`, `UserDevice`) are intentionally untouched per Requirement 16 and do not appear in any task.
- Task 10.1 (SilentRefreshMiddleware), task 4.2 (RotateAsync), and task 8.2 (AuthCookieWriter) are intentionally single large leaf tasks because their internal pieces are tightly coupled; splitting them further would create unsafe partial states in `Expense_Tracker.App` and `Expense_Tracker.Infrastructure`.
- Property-based tests use FsCheck.Xunit and are tagged with `[Trait("Feature", "cookie-based-auth-refactor")]` and `[Trait("Property", "N")]` per `design.md` §Testing Strategy.

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1", "1.2", "1.3", "2.1", "3.1", "5.1", "8.1", "9.1", "13.1", "14.1", "14.2", "21.1"] },
    { "id": 1, "tasks": ["1.4", "1.5", "2.2", "6.1", "7.1", "8.2", "9.2", "9.3", "14.3", "14.4", "14.5", "14.6"] },
    { "id": 2, "tasks": ["2.3", "4.1", "6.2", "6.3", "6.4", "6.5", "7.2", "7.3", "8.3", "8.4", "8.5", "11.1", "11.2", "12.1", "13.2", "15.1"] },
    { "id": 3, "tasks": ["2.4", "4.2", "10.1", "11.3", "11.4", "12.2", "12.3", "13.3", "15.2", "15.3"] },
    { "id": 4, "tasks": ["4.3", "4.4", "4.5", "4.6", "4.7", "4.8", "4.9", "4.10", "4.11", "4.12", "10.2", "10.3", "10.4", "10.5", "10.6", "10.7", "10.8", "10.9", "10.10", "10.11", "10.12", "10.13", "11.5", "11.6", "11.7", "12.4", "13.4", "16.1", "18.1"] },
    { "id": 5, "tasks": ["18.2", "18.3", "18.4", "18.5", "18.6", "18.7", "18.8", "18.9", "18.10", "19.1", "19.2", "19.3", "19.4", "19.5"] }
  ]
}
```
