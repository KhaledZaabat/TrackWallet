using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Expense_Tracker.App.Auth;

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
            if (!d.Secure)
                failures.Add(
                    $"Cookie '{d.Name}' must have Secure=true in non-Development environments."
                );

            if (string.IsNullOrWhiteSpace(d.Path))
                failures.Add($"Cookie '{d.Name}' must have a non-empty Path.");

            if ((int)d.SameSite < 0)
                failures.Add($"Cookie '{d.Name}' must have an explicit SameSite value.");
        }

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
