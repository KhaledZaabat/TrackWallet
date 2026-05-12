using System.Text.Json.Serialization;
using Expense_Tracker.App;
using Expense_Tracker.App.Auth;
using Expense_Tracker.App.Logging;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Application.Jobs;
using Expense_Tracker.Infrastructure.Data;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using HangfireBasicAuthenticationFilter;
using JasperFx.Resources;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using Serilog;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
  .Enrich.With<AuthTokenScrubber>() 
    .WriteTo.Console()
    .CreateBootstrapLogger();

builder.Host.UseSerilog(
    (ctx, services, cfg) =>
        cfg
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
           .Enrich.With<AuthTokenScrubber>() 
            .WriteTo.Console()
);

builder.Services.AddPresentation(builder.Configuration);

builder
    .Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressInferBindingSourcesForParameters = true;
});

var connectionString =
    builder.Configuration.GetConnectionString("PostgreSqlConnection")
    ?? throw new InvalidOperationException("PostgreSqlConnection is not configured");

builder.Host.UseWolverine(options =>
{
    options.Discovery.IncludeAssembly(typeof(IRepository<>).Assembly);

    options.PersistMessagesWithPostgresql(connectionString, "wolverine");

    options.UseEntityFrameworkCoreTransactions();

    options.Policies.AutoApplyTransactions();

    options.Services.AddResourceSetupOnStartup();

    options.DefaultExecutionTimeout = TimeSpan.FromMinutes(5);
    options.Policies.ConfigureConventionalLocalRouting();
});

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TrackWallet API V1");
    options.DocumentTitle = "TrackWallet API - Swagger UI";
});

FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.GetApplicationDefault() });

// Scalar
app.MapScalarApiReference(options =>
{
    options.Title = "TrackWallet API V1";
    options.OpenApiRoutePattern = "/swagger/{documentName}/swagger.json";
});

app.UseHangfireDashboard(
    "/jobs",
    new DashboardOptions
    {
        Authorization =
        [
            new HangfireCustomBasicAuthenticationFilter
            {
                User = app.Configuration["HangfireSettings:Username"],
                Pass = app.Configuration["HangfireSettings:Password"],
            },
        ],
    }
);

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.SeedAsync();

    RecurringJob.AddOrUpdate<RecordFamilyBudgetsJob>(
        "record-family-budgets-daily",
        job => job.ExecuteAsync(CancellationToken.None),
        Cron.Daily(0, 0),
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc }
    );
}

app.UseRouting();
app.UseCors("AllowFrontend");

// SilentRefreshMiddleware runs BEFORE UseAuthentication so it can inspect the cookies
// and, if rotation is required, inject the freshly minted access token into the request
// in time for UseAuthentication's first and only call to JwtBearerHandler. This avoids
// the pitfall of the JWT handler caching a NoResult outcome on a cookie-less request.
app.UseMiddleware<SilentRefreshMiddleware>();
app.UseAuthentication();
app.UseMiddleware<CsrfValidationMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();
