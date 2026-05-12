# Requirements Document

## Introduction

TrackWallet's backend (`Expense_Tracker.App` + `Expense_Tracker.Infrastructure`) serves a **browser-only web frontend**; there are no mobile or other non-browser clients, which is why the refactor commits fully to HttpOnly cookies as the single transport for authentication credentials. The flow today looks like this:

- `IdentityController` exposes `POST /api/identity/login`, `POST /api/identity/refresh`, and `POST /api/identity/logout`. These endpoints return an `AuthResponse` whose body contains a `JwtToken` and `RefreshToken` (`TokenResponse { Token, ExpiresAt }`). There is no external OAuth controller.
- `TokenProvider` (Infrastructure/Idenitity/TokenProvider.cs) mints the access token (HS256, 15 min, `JwtSettings.AccessTokenExpirationMinutes`) and generates a 64‑byte random base64 refresh token (90 days, `JwtSettings.RefreshTokenExpirationDays`). On every login/refresh it calls `RefreshTokenService.RevokeActiveTokensAsync` then `AddAsync` to persist a new row.
- `RefreshTokenService` (Infrastructure/Idenitity/RefreshTokenService.cs) stores refresh tokens **in plaintext** in the `RefreshTokens` table (`RefreshTokenConfiguration` has `HasIndex(x => x.Token).IsUnique()`, max length 500). `GetUserFromRefreshTokenAsync` looks a token up by `(Token, DeviceId)`, immediately revokes it, saves, and returns the user — so rotation exists but without reuse detection or hashing.
- JWT validation is configured by `JwtBearerOptionsConfigurator` with `ClockSkew = TimeSpan.Zero` and the default `JwtBearerHandler` reading from the `Authorization: Bearer` header. No `OnMessageReceived` hook exists today, so there is no way for the handler to read the token from a cookie.
- Request pipeline in `Program.cs` is `UseRouting → UseAuthentication → UseAuthorization → MapControllers`. There is no authentication middleware beyond the framework defaults, no silent refresh, no CSRF handling, and no endpoint-aware auth inspection (the `RequiresAuthorization(HttpContext)` helper mentioned in the refactor brief does not exist yet and must be introduced by the new middleware).
- `HttpUserContext` and `HttpFamilyContext` read claims (`ClaimTypes.NameIdentifier`, `CustomClaimTypes.FamilyId`, `CustomClaimTypes.IsParent`) from `HttpContext.User`, so any cookie-based scheme must still populate the same `ClaimsPrincipal`.
- Endpoint protection is entirely attribute-driven (`[Authorize]`, `[AllowAnonymous]`, custom filters `RequireFamilyAttribute`, `RequireParentRoleAttribute`). CORS policy `AllowFrontend` already permits `AllowCredentials()` from `http://localhost:3000`, but is currently disabled in `Program.cs`.

**Security weaknesses in the current design.** Tokens travel through JSON, so any XSS on the frontend exfiltrates them; refresh tokens are stored unhashed in the database; there is no reuse/theft detection, no CSRF protection (not needed today because no cookies are used), and no silent-refresh capability (clients must orchestrate refresh explicitly, which leaks the refresh token through their own storage).

**Refactor motivation.** Because the only consumer is a browser-based web frontend, the design moves both tokens into secure HttpOnly cookies, adds a silent refresh middleware that works transparently for `[Authorize]` endpoints, rotates and hashes refresh tokens, adds reuse detection, and introduces CSRF defenses — all without breaking existing `[Authorize]` attributes, authorization policies, filters, or the `ClaimsPrincipal`/`IUserContext`/`IFamilyContext` contracts consumed by every controller. No non-browser client surface needs to be preserved; the header-based bearer fallback is therefore **removed** in favor of a cookie-only contract.

## Glossary

- **Access Token Cookie**: An HttpOnly, Secure cookie carrying the short-lived JWT access token. Replaces the `JwtToken` field in today's `AuthResponse` body.
- **Refresh Token Cookie**: An HttpOnly, Secure cookie carrying the long-lived refresh token value. Replaces the `RefreshToken` field in today's `AuthResponse` body.
- **Cookie Authentication Integration**: The `JwtBearerEvents.OnMessageReceived` hook that reads the JWT out of the Access Token Cookie so the existing `JwtBearerHandler` can validate it and populate `HttpContext.User`.
- **Silent Refresh**: Server-side middleware behavior that transparently issues a new access token (and optionally a new refresh token) before the current one expires, without involving the client.
- **Silent Refresh Threshold**: The configurable remaining lifetime of the access token (e.g., 3 minutes) at or below which Silent Refresh triggers.
- **Refresh Rotation**: Issuing a new refresh token and invalidating the old one whenever a refresh occurs.
- **Reuse Detection**: Detecting a refresh attempt with an already-revoked (previously-rotated) refresh token, interpreted as token theft, which triggers revocation of the entire session/family chain.
- **Sliding Expiration**: Extending effective session lifetime by repeatedly issuing fresh access tokens (and optionally refreshing the refresh token window) while the user remains active, up to a configured absolute maximum.
- **Clock Skew**: Tolerance allowed between servers for JWT `exp`/`nbf` validation.
- **CSRF (Cross-Site Request Forgery)**: Attack where a malicious site causes the user's browser to submit an authenticated request. Because cookies are now auto-attached, CSRF defenses are required on state-changing endpoints.
- **CSRF Token**: A non-HttpOnly, same-origin-readable token that the frontend must echo back in a header on unsafe HTTP methods, validated by the server against the authenticated session.
- **HttpOnly**: Cookie flag that prevents JavaScript access via `document.cookie`.
- **Secure**: Cookie flag that restricts the cookie to HTTPS.
- **SameSite**: Cookie attribute (`Strict`, `Lax`, `None`) controlling cross-site sending.
- **Token_Provider**: The `ITokenProvider` implementation (`TokenProvider`) that mints JWTs and refresh tokens.
- **Refresh_Token_Service**: The `IRefreshTokenService` implementation (`RefreshTokenService`) that persists, revokes, and validates refresh tokens.
- **Auth_Cookie_Writer**: A new App-layer service that is the single source of truth for building `CookieOptions` and for writing and clearing **every** authentication-related cookie (Access Token Cookie, Refresh Token Cookie, CSRF Cookie, and any future session/correlation cookies introduced by auth flows) with production-safe defaults. All auth-related cookie attributes (HttpOnly, Secure, SameSite, Path, Domain, Expires, Name) MUST be produced here so they cannot drift between call sites; direct `Response.Cookies.Append`/`Delete` for auth-related cookies is disallowed per Requirement 22.
- **Silent_Refresh_Middleware**: The new ASP.NET Core middleware that inspects authenticated requests and performs Silent Refresh when conditions are met.
- **Endpoint_Auth_Inspector**: The helper (equivalent to the `RequiresAuthorization(HttpContext)` snippet in the brief) that inspects `HttpContext.GetEndpoint()` for `IAuthorizeData`/`IAllowAnonymous` to decide whether an endpoint requires authentication.
- **Session_Family_Id**: An identifier grouping all rotated refresh tokens that descend from the same original login, used by Reuse Detection to revoke an entire lineage.
- **User_Context**: `IUserContext` (`HttpUserContext`) — the abstraction the application layer uses to read the current user id from claims.
- **Family_Context**: `IFamilyContext` (`HttpFamilyContext`) — the abstraction that reads family claims (`CustomClaimTypes.FamilyId`, `CustomClaimTypes.IsParent`).

## Requirements

### Requirement 1: Remove tokens from JSON response bodies

**User Story:** As a security engineer, I want authentication endpoints to stop returning access and refresh tokens in JSON bodies, so that XSS-level attackers cannot read tokens from the page.

#### Acceptance Criteria

1. WHEN `POST /api/identity/login` succeeds, THE Identity_Controller SHALL return an `AuthResponse` body whose serialized JSON contains no access token value and no refresh token value.
2. WHEN `POST /api/identity/refresh` succeeds, THE Identity_Controller SHALL return a response body whose serialized JSON contains no access token value and no refresh token value.
3. THE AuthResponse contract SHALL expose only non-token user-facing fields (`UserId`, `Email`, `FullName`, `ProfileImageUrl`, `Families`).
4. IF any controller attempts to serialize a field of type `TokenResponse` into an HTTP response body, THEN the build SHALL fail or the response pipeline SHALL reject the write (enforced by contract, review, or a serialization guard).

### Requirement 2: Issue the Access Token Cookie on successful authentication

**User Story:** As a frontend developer, I want the access token to be delivered as a secure cookie so that the browser attaches it automatically without any client-side token handling.

This requirement's cookie attributes MUST be produced by Auth_Cookie_Writer per Requirement 22.

#### Acceptance Criteria

1. WHEN `POST /api/identity/login` succeeds, THE Auth_Cookie_Writer SHALL set the Access Token Cookie on the response.
2. WHEN `POST /api/identity/refresh` succeeds, THE Auth_Cookie_Writer SHALL set the Access Token Cookie on the response.
3. WHEN the Access Token Cookie is written, THE Auth_Cookie_Writer SHALL set `HttpOnly=true`, `Secure=true`, `SameSite` to the configured value (default `Strict`, configurable per environment), `Path=/`, and `Expires`/`Max-Age` equal to the JWT `exp` claim (derived from `JwtSettings.AccessTokenExpirationMinutes`).
4. THE Access Token Cookie name SHALL be distinct from the Refresh Token Cookie name and SHALL be configurable via `JwtSettings` (or a dedicated cookie settings section).
5. WHERE the environment is `Development`, THE Auth_Cookie_Writer SHALL allow `Secure=false` only when an explicit configuration flag opts in; otherwise `Secure` SHALL remain `true`.

### Requirement 3: Issue the Refresh Token Cookie on successful authentication

**User Story:** As a security engineer, I want refresh tokens delivered only as HttpOnly cookies so that they are never exposed to JavaScript or application storage.

This requirement's cookie attributes MUST be produced by Auth_Cookie_Writer per Requirement 22.

#### Acceptance Criteria

1. WHEN `POST /api/identity/login` succeeds, THE Auth_Cookie_Writer SHALL set the Refresh Token Cookie on the response.
2. WHEN `POST /api/identity/refresh` succeeds, THE Auth_Cookie_Writer SHALL set the Refresh Token Cookie on the response.
3. WHEN the Refresh Token Cookie is written, THE Auth_Cookie_Writer SHALL set `HttpOnly=true`, `Secure=true`, `SameSite` to the configured value (default `Strict`), `Path` scoped to the refresh-related routes (minimum `/api/identity`), and `Expires`/`Max-Age` equal to the refresh token expiration (derived from `JwtSettings.RefreshTokenExpirationDays`).
4. THE Refresh Token Cookie SHALL have a distinct, non-guessable name configurable via settings.
5. THE Refresh Token Cookie value SHALL be the opaque refresh token (not a JWT) produced by Token_Provider.

### Requirement 4: Read the JWT from the Access Token Cookie during authentication

**User Story:** As a backend engineer, I want the JWT bearer authentication to read the token exclusively from the cookie so that `[Authorize]` and `ClaimsPrincipal` continue to work unchanged and no alternate transport can be used to authenticate.

#### Acceptance Criteria

1. WHEN an incoming request has an Access Token Cookie, THE Jwt_Bearer_Options_Configurator SHALL, via `JwtBearerEvents.OnMessageReceived`, set `context.Token` to the cookie value.
2. WHEN an incoming request has an `Authorization: Bearer <token>` header but no Access Token Cookie, THE Jwt_Bearer_Options_Configurator SHALL NOT use the header value; `context.Token` SHALL remain unset so the request is treated as unauthenticated on `[Authorize]` endpoints.
3. WHEN the Access Token Cookie is present and the JWT is valid, THE Jwt_Bearer_Handler SHALL populate `HttpContext.User` with the same claims previously produced by `TokenProvider.BuildClaims` (including `sub`, `CustomClaimTypes.UserId`, `CustomClaimTypes.Email`, `CustomClaimTypes.FamilyId`, `CustomClaimTypes.IsParent`).
4. IF the Access Token Cookie is missing on an endpoint that requires authorization, THEN THE Jwt_Bearer_Handler SHALL respond with `401 Unauthorized` per the default challenge behavior, regardless of whether an `Authorization` header is present.

### Requirement 5: Silent refresh middleware behavior

**User Story:** As an end user, I want my session to stay alive silently as long as I'm active, so that I never see a logout or re-login prompt mid-session.

#### Acceptance Criteria

1. WHEN a request reaches Silent_Refresh_Middleware and the access token's remaining lifetime is less than or equal to the configured Silent Refresh Threshold (default 3 minutes, configurable in `JwtSettings`), THE Silent_Refresh_Middleware SHALL attempt a silent refresh.
2. WHEN the access token's remaining lifetime is greater than the Silent Refresh Threshold, THE Silent_Refresh_Middleware SHALL pass the request through without touching the database or the tokens.
3. WHEN a silent refresh succeeds, THE Silent_Refresh_Middleware SHALL mint a new access token, rotate the refresh token, rewrite both cookies on the response, and continue processing the request as authenticated with the new principal.
4. WHEN a silent refresh succeeds, THE Silent_Refresh_Middleware SHALL return the original endpoint's response (no redirect, no client-visible indication of the refresh).
5. IF the refresh token from the Refresh Token Cookie is missing, expired, revoked, or invalid, THEN THE Silent_Refresh_Middleware SHALL clear both auth cookies and let the request continue to the default `401 Unauthorized` challenge.
6. IF Silent_Refresh_Middleware has already run for the current `HttpContext`, THEN it SHALL not run a second time for the same request (loop prevention via a per-request marker).

### Requirement 6: Endpoint-aware middleware activation

**User Story:** As a backend engineer, I want silent refresh to run only where it is needed, so that anonymous and static endpoints are not slowed down or mutated.

#### Acceptance Criteria

1. WHEN a request has no routing endpoint (`HttpContext.GetEndpoint() is null`), THE Silent_Refresh_Middleware SHALL pass through without attempting refresh.
2. WHEN the matched endpoint has `IAllowAnonymous` metadata, THE Silent_Refresh_Middleware SHALL pass through without attempting refresh.
3. WHEN the matched endpoint has no `IAuthorizeData` metadata, THE Silent_Refresh_Middleware SHALL pass through without attempting refresh.
4. WHERE the endpoint requires authorization (has `IAuthorizeData` and does not have `IAllowAnonymous`), THE Silent_Refresh_Middleware SHALL evaluate the Silent Refresh Threshold and proceed per Requirement 5.
5. THE Endpoint_Auth_Inspector SHALL be exposed as a reusable static helper so that other middleware or diagnostics can share the same "requires authorization" decision.

### Requirement 7: Avoid unnecessary database access per request

**User Story:** As a performance engineer, I want auth middleware to avoid hitting the database on every authenticated request, so that latency and DB load stay low.

#### Acceptance Criteria

1. WHEN Silent_Refresh_Middleware decides not to rotate (remaining lifetime above threshold, anonymous endpoint, or unmatched endpoint), THE Silent_Refresh_Middleware SHALL not execute any query against `AppDbContext.RefreshTokens`.
2. WHEN Silent_Refresh_Middleware performs a rotation, THE Refresh_Token_Service SHALL execute at most one read and one write transaction for the rotation path.
3. THE Refresh_Token_Service SHALL expose a lookup API that queries by the hashed refresh token value plus `DeviceId` in a single indexed query.
4. WHERE a short-lived cache (e.g., `IMemoryCache`) is used to record "refresh already happened for this token within N seconds", THE Silent_Refresh_Middleware SHALL consult the cache before touching the database, with a TTL shorter than the Silent Refresh Threshold.

### Requirement 8: Concurrency-safe refresh rotation

**User Story:** As an end user on a flaky network, I want parallel in-flight requests to refresh correctly without being logged out, so that bursts of requests do not trigger false reuse detection.

#### Acceptance Criteria

1. WHEN two or more concurrent requests for the same user and `DeviceId` trigger Silent Refresh simultaneously, THE Refresh_Token_Service SHALL ensure exactly one rotation succeeds and the other concurrent rotations either reuse the just-issued refresh token or receive the same new access token.
2. THE Refresh_Token_Service SHALL implement the rotation as a single atomic database operation (e.g., row-level lock, optimistic concurrency token, or `UPDATE ... WHERE RevokedAt IS NULL AND Token = @hash RETURNING ...`) that prevents double-revoke races.
3. WHERE a rotation loses the race, THE Silent_Refresh_Middleware SHALL accept the winner's newly issued tokens (read-after-write within a short grace window) instead of treating the request as a reuse attack.
4. WHEN concurrent rotations resolve, THE Silent_Refresh_Middleware SHALL write the cookies corresponding to the winning rotation only.

### Requirement 9: Refresh token rotation, hashing, and storage

**User Story:** As a security engineer, I want refresh tokens hashed at rest and rotated on every use, so that a stolen database dump cannot impersonate users and a stolen refresh token is only usable once.

#### Acceptance Criteria

1. THE Refresh_Token_Service SHALL store refresh tokens as a cryptographic hash (e.g., SHA-256) of the raw token value, never the raw value itself.
2. WHEN a refresh token is validated, THE Refresh_Token_Service SHALL hash the incoming value and compare it to the stored hash in a single indexed query.
3. WHEN a refresh succeeds, THE Refresh_Token_Service SHALL revoke the incoming refresh token row and insert a new refresh token row with a new hash and a new value inside the same transaction.
4. THE `RefreshToken` entity SHALL record a `Session_Family_Id` that is preserved across rotations within the same login session and a `ReplacedByTokenId` pointer linking a revoked token to its successor.
5. THE existing plaintext `Token` column SHALL be replaced or supplemented by a `TokenHash` column with a unique index; the migration SHALL be additive (no data preserved from plaintext values, since they cannot be re-hashed).

### Requirement 10: Reuse detection and session revocation

**User Story:** As a security engineer, I want reused (already-rotated) refresh tokens to trigger full session revocation, so that token theft cannot be sustained.

#### Acceptance Criteria

1. IF an incoming refresh token matches a stored token whose `RevokedAt` is not null, THEN THE Refresh_Token_Service SHALL treat the request as a reuse attack.
2. WHEN a reuse attack is detected, THE Refresh_Token_Service SHALL revoke every refresh token sharing the same `Session_Family_Id` (both active and future descendants) in a single transaction.
3. WHEN a reuse attack is detected, THE Silent_Refresh_Middleware SHALL clear both auth cookies and let the request fall through to `401 Unauthorized`.
4. WHEN a reuse attack is detected, THE Refresh_Token_Service SHALL log a security event containing `UserId`, `DeviceId`, `Session_Family_Id`, request IP, and user agent — with no token values in the log.

### Requirement 11: Sliding expiration strategy

**User Story:** As an end user, I want my session to extend as long as I stay active, but expire after a reasonable absolute maximum, so that inactivity logs me out without interrupting active use.

#### Acceptance Criteria

1. WHEN a refresh rotation occurs, THE Refresh_Token_Service SHALL issue a new refresh token whose expiration is `now + RefreshTokenExpirationDays`.
2. THE Refresh_Token_Service SHALL enforce an absolute maximum session lifetime measured from the original login timestamp of the Session_Family_Id, configurable via `JwtSettings.AbsoluteSessionLifetimeDays`.
3. IF a rotation would extend a session beyond the absolute maximum, THEN THE Refresh_Token_Service SHALL reject the rotation and the Silent_Refresh_Middleware SHALL clear cookies and return to the default `401` challenge.
4. THE sliding behavior SHALL apply to the Refresh Token Cookie's `Expires` attribute as well, so the cookie's lifetime tracks the token's lifetime on each rotation.

### Requirement 12: CSRF defenses for cookie-based authentication

**User Story:** As a security engineer, I want CSRF protection on state-changing endpoints, so that automatic cookie attachment from third-party origins cannot forge authenticated actions.

This requirement's cookie attributes MUST be produced by Auth_Cookie_Writer per Requirement 22.

#### Acceptance Criteria

1. WHEN the application starts, THE Program SHALL register ASP.NET Core `IAntiforgery` services with a non-HttpOnly CSRF cookie (e.g., `XSRF-TOKEN`) and a header name (e.g., `X-XSRF-TOKEN`).
2. WHEN a client receives any authenticated response, THE Auth_Cookie_Writer SHALL also ensure the non-HttpOnly CSRF cookie is set/refreshed.
3. WHEN an authenticated request uses an unsafe HTTP method (`POST`, `PUT`, `PATCH`, `DELETE`) on an endpoint that requires authorization, THE CSRF middleware/filter SHALL validate the `X-XSRF-TOKEN` header against the CSRF cookie.
4. IF CSRF validation fails on an unsafe authenticated request, THEN THE CSRF middleware SHALL short-circuit the pipeline with `403 Forbidden` and no auth side effects (no rotation, no cookie writes).
5. WHERE the request path is explicitly allow-listed for CSRF exemption (e.g., `POST /api/identity/login`, `POST /api/identity/refresh`, `POST /api/identity/register` — endpoints that precede login establishment), THE CSRF middleware SHALL skip validation.
6. THE SameSite attribute on the Access Token Cookie and the Refresh Token Cookie SHALL default to `Strict` as an additional CSRF mitigation layer.

### Requirement 13: Clock skew handling

**User Story:** As a backend engineer, I want JWT validation to tolerate small clock drift between servers, so that the system does not reject valid tokens from correctly-configured clients.

#### Acceptance Criteria

1. THE Jwt_Bearer_Options_Configurator SHALL set `TokenValidationParameters.ClockSkew` to a configurable value (default 30 seconds) sourced from `JwtSettings.ClockSkewSeconds`.
2. THE Silent_Refresh_Middleware SHALL compute "remaining lifetime" relative to UTC `DateTime.UtcNow` and SHALL use the same configured clock skew when comparing against the Silent Refresh Threshold.
3. THE token-parsing path used by `TokenProvider.GetPrincipalFromExpiredToken` SHALL also honor the configured clock skew.

### Requirement 14: Logout invalidates cookies and server-side state

**User Story:** As an end user, I want logout to fully terminate my session so that no leftover token can be replayed.

This requirement's cookie attributes MUST be produced by Auth_Cookie_Writer per Requirement 22.

#### Acceptance Criteria

1. WHEN `POST /api/identity/logout` is invoked by an authenticated user, THE Logout_Command_Handler SHALL revoke all active refresh tokens for the current `UserId` and `DeviceId` via `Refresh_Token_Service.RevokeActiveTokensAsync`.
2. WHEN `POST /api/identity/logout` is invoked by an authenticated user, THE Auth_Cookie_Writer SHALL clear the Access Token Cookie, the Refresh Token Cookie, and the CSRF cookie by writing expired cookies with the same `Name`, `Path`, `Domain`, `Secure`, `SameSite`, and `HttpOnly` attributes used when they were set.
3. IF logout is called without authentication, THEN THE Identity_Controller SHALL return `401 Unauthorized` without clearing any cookies or writing any DB state.
4. WHEN logout succeeds, THE Silent_Refresh_Middleware SHALL not attempt to refresh on the same response (logout takes precedence; no re-issuing of cookies after clearing).

### Requirement 15: Refresh endpoint continues to work without request body tokens

**User Story:** As a frontend developer, I want a dedicated refresh endpoint that uses only the Refresh Token Cookie, so that I never need to send the refresh token in a JSON body.

#### Acceptance Criteria

1. WHEN `POST /api/identity/refresh` is invoked, THE Identity_Controller SHALL read the refresh token value from the Refresh Token Cookie only and SHALL ignore any refresh token value in the request body.
2. THE `RefreshTokenRequest` contract SHALL no longer require a `RefreshToken` field; the remaining fields (e.g., `DeviceId`, `FcmToken`) MAY remain in the body.
3. IF the Refresh Token Cookie is missing, THEN THE Identity_Controller SHALL return `401 Unauthorized`.
4. WHEN the refresh succeeds, THE Auth_Cookie_Writer SHALL rotate both auth cookies per Requirements 2, 3, 9, and 11.
5. WHEN the refresh succeeds, THE Identity_Controller SHALL return an `AuthResponse` body (same non-token fields defined in Requirement 1) and a `200 OK` status.

### Requirement 16: Preserve `[Authorize]`, policies, filters, and claims-based authorization

**User Story:** As a backend engineer maintaining controllers, I want existing authorization attributes and filters to keep working unchanged, so that no controller code needs to be touched outside of the auth refactor.

#### Acceptance Criteria

1. WHEN the refactor is complete, THE controllers (`BudgetController`, `CategoriesController`, `DashboardController`, `FamiliesController`, `FamilyTransactionsController`, `FilesController`, `InvitationsController`, `NotificationPreferencesController`, `UserController`, `UserDevice`) SHALL compile and behave with zero `[Authorize]`, `[AllowAnonymous]`, or `[ApiController]` attribute changes.
2. THE `RequireFamilyAttribute` and `RequireParentRoleAttribute` filters SHALL continue to receive a populated `HttpContext.User` with the same `CustomClaimTypes.FamilyId`/`CustomClaimTypes.IsParent` claims they read today.
3. THE `IUserContext` (`HttpUserContext`) and `IFamilyContext` (`HttpFamilyContext`) implementations SHALL return the same values as before for the same authenticated user.
4. THE policy-based authorization registrations (if any are added later) SHALL evaluate against the same `ClaimsPrincipal` produced from the Access Token Cookie.

### Requirement 17: Clean architecture and separation of concerns

**User Story:** As a backend engineer reviewing the diff, I want the new code to fit the existing Clean Architecture layout, so that Domain, Application, Infrastructure, and App responsibilities stay clear.

#### Acceptance Criteria

1. THE Auth_Cookie_Writer and cookie option model SHALL live in the App layer (`Expense_Tracker.App`), because they are HTTP/framework concerns.
2. THE Silent_Refresh_Middleware and Endpoint_Auth_Inspector SHALL live in the App layer next to other HTTP middleware, and SHALL be registered in `Program.cs` between `UseAuthentication` and `UseAuthorization`.
3. THE refresh token hashing, rotation, reuse detection, and Session_Family_Id logic SHALL live in the Infrastructure layer (`Expense_Tracker.Infrastructure/Idenitity`), behind the existing `IRefreshTokenService` abstraction.
4. THE `ITokenProvider` and `IRefreshTokenService` abstractions SHALL remain in the Application layer, with signatures extended rather than leaked implementation types.
5. THE new cookie/CSRF configuration options SHALL be bound via `AddOptions<T>().BindConfiguration(...)` in `DependencyInjection.cs`, matching the existing pattern for `JwtSettings` and `OtpSettings`.
6. THE refactor SHALL not introduce new cross-layer dependencies (App ↔ Domain, Infrastructure ↔ App).

### Requirement 18: Security non-functional requirements

**User Story:** As a security engineer, I want measurable security guarantees so that the refactor can be audited.

#### Acceptance Criteria

1. THE production configuration SHALL reject startup if the Access Token Cookie or Refresh Token Cookie `Secure` flag is `false` while `ASPNETCORE_ENVIRONMENT=Production`.
2. WHEN a refresh token is compared, THE Refresh_Token_Service SHALL use a constant-time comparison on the hash to avoid timing side channels.
3. THE refresh token raw value SHALL be at least 256 bits of cryptographically secure randomness (`RandomNumberGenerator.GetBytes(32)` or stronger, matching or exceeding the current 64-byte base64 value).
4. THE access token and refresh token cookies SHALL NOT be logged, traced, or written to Serilog/ILogger output anywhere in the pipeline.
5. THE refresh token hash SHALL be a one-way function; reversing a stored hash to the raw value SHALL not be possible by any service in the application.
6. WHEN CORS is enabled for the frontend, THE CORS policy SHALL list explicit allowed origins and `AllowCredentials()`, and SHALL not use `AllowAnyOrigin()` together with credentials.

### Requirement 19: Performance non-functional requirements

**User Story:** As a performance engineer, I want the new middleware to keep the request pipeline fast for typical traffic.

#### Acceptance Criteria

1. WHEN the access token's remaining lifetime is above the Silent Refresh Threshold, THE Silent_Refresh_Middleware's added latency per request SHALL be dominated by an in-memory JWT `exp` read (no DB call, no allocation of a `DbContext`).
2. WHEN Silent_Refresh_Middleware rotates, THE total added latency SHALL be at most one indexed query and one transactional write against `RefreshTokens` plus the cookie write.
3. THE middleware SHALL not resolve `IRefreshTokenService` or `AppDbContext` from DI on the non-rotating path (lazy resolution only when a rotation is actually needed).

### Requirement 20: Backward compatibility and migration

**User Story:** As a release engineer, I want the refactor to ship cleanly, so that deployed clients and historical data migrate without data loss or downtime.

#### Acceptance Criteria

1. WHEN the new build is deployed, THE database migration SHALL add the `TokenHash`, `Session_Family_Id`, and `ReplacedByTokenId` columns (and their indexes) to the `RefreshTokens` table without dropping the table.
2. WHEN the migration runs, THE existing active refresh token rows SHALL be marked revoked (because their plaintext values cannot be re-hashed equivalently) so that all sessions are forced to re-login once, safely.
3. THE `AuthResponse` contract consumed by the frontend SHALL remain compatible in the sense that removed token fields are treated as optional and the non-token fields (`UserId`, `Email`, `FullName`, `ProfileImageUrl`, `Families`) stay at their current paths.
4. WHEN the frontend receives the new response, THE frontend SHALL NOT be required to read or store any token value (this is enforced by Requirement 1 and is noted here as a compatibility boundary).
5. THE backend SHALL assume a browser-only client surface; non-browser clients are out of scope, and the `Authorization: Bearer` header SHALL NOT be accepted as an authentication source on any endpoint after the refactor.

### Requirement 21: Device/session awareness preserved

**User Story:** As an end user with multiple devices, I want logging out or being compromised on one device not to log me out on another, so that multi-device usage continues to work.

#### Acceptance Criteria

1. THE Refresh_Token_Service SHALL continue to key refresh tokens by `(UserId, DeviceId)` so that rotation and reuse detection are scoped per device.
2. WHEN a reuse attack is detected on one device, THE Refresh_Token_Service SHALL revoke only the Session_Family_Id lineage for that `DeviceId`, not other devices' Session_Family_Ids for the same user.
3. WHEN logout is invoked, THE Logout_Command_Handler SHALL revoke only the current device's active tokens (matching today's behavior).
4. WHERE an admin-initiated "logout everywhere" capability is added later, THE Refresh_Token_Service SHALL expose a method to revoke every Session_Family_Id for a `UserId` in one call.

### Requirement 22: Consistent secure cookie attributes across all authentication-related cookies

**User Story:** As a security engineer, I want every cookie produced by authentication flows (access, refresh, CSRF, and any future session or correlation cookies) to carry the same centrally-defined secure attributes, so that security posture cannot silently drift between cookies or between call sites.

#### Acceptance Criteria

1. THE set of "authentication-related cookies" SHALL be explicitly defined as: the Access Token Cookie, the Refresh Token Cookie, the CSRF Cookie, and any future session, correlation, or auth-state cookie introduced by authentication or refresh flows; every cookie in this set SHALL be subject to Requirements 22.2 through 22.10.
2. THE Auth_Cookie_Writer SHALL be the single centralized component that builds `CookieOptions` for authentication-related cookies; no other component, controller, middleware, or handler SHALL construct `CookieOptions` for an authentication-related cookie.
3. WHEN any authentication-related cookie other than the CSRF Cookie is written, THE Auth_Cookie_Writer SHALL set `HttpOnly=true`.
4. WHEN the CSRF Cookie is written, THE Auth_Cookie_Writer SHALL set `HttpOnly=false`, because the frontend must read the CSRF token value via same-origin JavaScript in order to echo it back in the `X-XSRF-TOKEN` header; this is the only documented exception to the `HttpOnly=true` rule in Requirement 22.3.
5. WHEN any authentication-related cookie is written, THE Auth_Cookie_Writer SHALL set `Secure=true` in every environment, with the single exception of the `Development` opt-out already defined in Requirement 2.5 (an explicit configuration flag); no other environment SHALL be permitted to disable `Secure`.
6. WHEN any authentication-related cookie is written, THE Auth_Cookie_Writer SHALL set `SameSite` to a value resolved from configuration, defaulting to `Strict`, configurable per cookie via settings, and documented per cookie (for example, the CSRF Cookie MAY be configured to `Lax` where cross-subdomain navigation is required).
7. WHEN any authentication-related cookie is written, THE Auth_Cookie_Writer SHALL set `Path`, `Domain`, and `Expires` (or `Max-Age`) explicitly from configuration or from the token lifetime; none of these attributes SHALL be left to framework defaults.
8. WHEN the application starts in any environment other than `Development`, THE Program SHALL execute a startup-time assertion that validates every registered authentication-related cookie's `CookieOptions` against Requirements 22.3 through 22.7 (including the HttpOnly exception for the CSRF Cookie), and SHALL fail fast (prevent startup) on any misconfiguration.
9. WHEN any authentication-related cookie is cleared (logout, reuse detection, invalid refresh, or any other clearing path), THE Auth_Cookie_Writer SHALL emit the clearing instruction using the exact same `Name`, `Path`, `Domain`, `Secure`, `SameSite`, and `HttpOnly` values that were used when the cookie was set, so that browsers actually delete the cookie.
10. IF any code path attempts to introduce a new authentication-related cookie without routing the write through Auth_Cookie_Writer (for example, a direct `Response.Cookies.Append` or `Response.Cookies.Delete` for an auth-related cookie name), THEN the change SHALL be rejected by convention and code review; no authentication-related cookie SHALL be written or cleared outside of Auth_Cookie_Writer.
