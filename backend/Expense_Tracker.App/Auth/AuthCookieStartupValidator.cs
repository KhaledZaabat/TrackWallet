using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Expense_Tracker.App.Auth;

/// <summary>
/// Startup guard that fails the host if any registered auth cookie violates the
/// production security invariants (R18.1, R22.3–R22.8). Skipped in Development
/// because <c>AuthCookieOptions.AllowInsecureInDevelopment</c> may legitimately
/// permit <c>Secure=false</c> there (R2.5).
/// </summary>
/// <remarks>
/// <see cref="IAuthCookieWriter"/> is scoped (registered via <c>IScopedService</c>),
/// so this hosted service resolves it through <see cref="IServiceScopeFactory"/>
/// to avoid the captive-dependency validation failure that singleton-to-scoped
/// injection would trigger at design time and at runtime.
/// </remarks>
public sealed class AuthCookieStartupValidator : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IWebHostEnvironment _env;

    public AuthCookieStartupValidator(IServiceScopeFactory scopeFactory, IWebHostEnvironment env)
    {
        _scopeFactory = scopeFactory;
        _env = env;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_env.IsDevelopment())
            return Task.CompletedTask;

        using IServiceScope scope = _scopeFactory.CreateScope();
        IAuthCookieWriter writer = scope.ServiceProvider.GetRequiredService<IAuthCookieWriter>();

        IReadOnlyList<AuthCookieDescriptor> descriptors = writer.GetRegisteredDescriptors();
        List<string> failures = new();

        foreach (AuthCookieDescriptor d in descriptors)
        {
            // R22.5 / R18.1 — Secure must be true outside Development.
            if (!d.Secure)
                failures.Add(
                    $"Cookie '{d.Name}' must have Secure=true in non-Development environments."
                );

            // R22.7 — Path must be explicitly set.
            if (string.IsNullOrWhiteSpace(d.Path))
                failures.Add($"Cookie '{d.Name}' must have a non-empty Path.");

            // Note: SameSite is a non-nullable enum, so "explicit" is guaranteed by the type.
            // A defensive check against the default enum value (SameSiteMode.Unspecified = -1)
            // is still useful because Microsoft.AspNetCore.Http.SameSiteMode.Unspecified means
            // "let the browser choose", which is not what we want (R22.6).
            if ((int)d.SameSite < 0)
                failures.Add($"Cookie '{d.Name}' must have an explicit SameSite value.");
        }

        // HttpOnly partitioning: access + refresh must be HttpOnly, CSRF must not be (R22.3, R22.4).
        int httpOnlyCount = 0;
        int nonHttpOnlyCount = 0;
        foreach (AuthCookieDescriptor d in descriptors)
        {
            if (d.HttpOnly)
                httpOnlyCount++;
            else
                nonHttpOnlyCount++;
        }

        if (httpOnlyCount < 2)
            failures.Add("Expected at least two HttpOnly auth cookies (access and refresh).");

        if (nonHttpOnlyCount < 1)
            failures.Add("Expected at least one non-HttpOnly auth cookie (CSRF).");

        if (failures.Count > 0)
        {
            throw new InvalidOperationException(
                "Auth cookie startup validation failed: " + string.Join("; ", failures)
            );
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
