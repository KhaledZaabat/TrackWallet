using Expense_Tracker.App;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Application.Jobs;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPresentation(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressInferBindingSourcesForParameters = true;
});

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TrackWallet API V1");
    options.DocumentTitle = "TrackWallet API - Swagger UI";
});



FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.GetApplicationDefault()

});


// Scalar
app.MapScalarApiReference(options =>
{
    options.Title = "TrackWallet API V1";
    options.OpenApiRoutePattern = "/swagger/{documentName}/swagger.json";
});

app.UseHangfireDashboard("/jobs", new DashboardOptions
{
    Authorization =
    [
        new HangfireCustomBasicAuthenticationFilter
        {
            User = app.Configuration["HangfireSettings:Username"],
            Pass = app.Configuration["HangfireSettings:Password"]
        }
    ]
});

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
//app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
