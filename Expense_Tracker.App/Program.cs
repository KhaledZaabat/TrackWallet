using Hangfire;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Mvc;
using Expense_Tracker.App;
using Expense_Tracker.Application.Interfaces;
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
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "QuizFlow API V1");
    options.DocumentTitle = "QuizFlow API - Swagger UI";
});



// Scalar
app.MapScalarApiReference(options =>
{
    options.Title = "QuizFlow API V1";
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
}

app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
