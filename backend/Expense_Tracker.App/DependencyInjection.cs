using System.Reflection;
using Asp.Versioning;
using Expense_Tracker.App.Auth;
using Expense_Tracker.App.Implemntation;
using Expense_Tracker.Application.Common.Settings;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common;
using Expense_Tracker.Infrastructure.Data;
using Expense_Tracker.Infrastructure.Data.Interceptors;
using Expense_Tracker.Infrastructure.Email;
using Expense_Tracker.Infrastructure.Idenitity;
using Expense_Tracker.Infrastructure.Services;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Resend;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Expense_Tracker.App;

public static class ServiceRegistration
{
    public const string CorsPolicyName = "AllowFrontend";

    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHttpContextAccessor();

        return services
            .AddInfrastructure(configuration)
            .AddAppOptions()
            .AddAutoRegisteredServices()
            .AddIdentityAndAuth(configuration)
            .AddApi()
            .AddCors(BuildCorsPolicy)
            .AddCache()
            .AddEmailSending(configuration)
            .AddBackgroundJobs(configuration)
            .ConfigureForwardedHeaders()
            .AddObjectMapping()
            .AddProblemDetailsPipeline()
            .AddRequestContexts();
    }

    // --- Infrastructure ----------------------------------------------------

    private static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddScoped<UpdatableEntityInterceptor>();
        services.AddScoped<CreatableEntityInterceptor>();
        services.AddScoped<SoftDeleteEntityInterceptor>();

        string? connectionString = configuration.GetConnectionString("PostgreSqlConnection");

        services.AddDbContext<AppDbContext>(
            (sp, options) =>
                options
                    .UseNpgsql(connectionString)
                    .AddInterceptors(
                        sp.GetRequiredService<CreatableEntityInterceptor>(),
                        sp.GetRequiredService<UpdatableEntityInterceptor>(),
                        sp.GetRequiredService<SoftDeleteEntityInterceptor>()
                    )
        );

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }

    private static IServiceCollection AddCache(this IServiceCollection services)
    {
        services.AddMemoryCache();
        return services;
    }

    private static IServiceCollection AddEmailSending(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<ResendSettings>(configuration.GetSection("Resend"));

        services.AddHttpClient<ResendClient>();
        services.Configure<ResendClientOptions>(o => o.ApiToken = configuration["Resend:ApiKey"]!);

        services.AddTransient<IResend, ResendClient>();
        services.AddScoped<IEmailSender, ResendEmailSender>();

        return services;
    }

    private static IServiceCollection AddBackgroundJobs(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHangfire(cfg =>
            cfg.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(o =>
                    o.UseNpgsqlConnection(
                        configuration.GetConnectionString("HangfirePostgreConnection")
                    )
                )
        );

        services.AddHangfireServer();
        return services;
    }

    // --- Options binding ---------------------------------------------------

    private static IServiceCollection AddAppOptions(this IServiceCollection services)
    {
        services
            .AddOptions<EmailLinkOptions>()
            .BindConfiguration(EmailLinkOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<FileUrlOptions>()
            .BindConfiguration(FileUrlOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    // --- DI auto-registration ---------------------------------------------

    private static IServiceCollection AddAutoRegisteredServices(this IServiceCollection services)
    {
        services.Scan(scan =>
            scan.FromAssembliesOf(typeof(AppDbContext), typeof(ServiceRegistration))
                .AddClasses(c => c.AssignableTo<IScopedService>())
                .AsImplementedInterfaces()
                .WithScopedLifetime()
                .AddClasses(c => c.AssignableTo<ITransientService>())
                .AsImplementedInterfaces()
                .WithTransientLifetime()
                .AddClasses(c => c.AssignableTo<ISingletonService>())
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
        );

        return services;
    }

    // --- Identity, JWT, cookies --------------------------------------------

    private static IServiceCollection AddIdentityAndAuth(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // Confirmation / reset / 2FA tokens — 15 min beats Identity's 24h default.
        services.Configure<DataProtectionTokenProviderOptions>(o =>
            o.TokenLifespan = TimeSpan.FromMinutes(15)
        );

        services.AddJwt(configuration);
        services.AddCookieAuth();
        return services;
    }

    private static IServiceCollection AddJwt(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddOptions<JwtSettings>()
            .BindConfiguration(JwtSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(sp => sp.GetRequiredService<IOptions<JwtSettings>>().Value);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

        services.ConfigureOptions<JwtBearerOptionsConfigurator>();

        services
            .AddAuthentication()
            .AddGoogle(options =>
            {
                options.ClientId =
                    configuration["Authentication:Google:ClientId"]
                    ?? throw new InvalidOperationException("Google ClientId is missing");
                options.ClientSecret =
                    configuration["Authentication:Google:ClientSecret"]
                    ?? throw new InvalidOperationException("Google ClientSecret is missing");

                options.CallbackPath = "/signin-google";
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.SaveTokens = true;

                options.Scope.Add("email");
                options.Scope.Add("profile");
            });

        return services;
    }

    private static IServiceCollection AddCookieAuth(this IServiceCollection services)
    {
        services
            .AddOptions<AuthCookieOptions>()
            .BindConfiguration(AuthCookieOptions.SectionName)
            .ValidateDataAnnotations()
            .Validate(
                o => o.AccessCookieName != o.RefreshCookieName,
                "Access and Refresh cookie names must differ."
            )
            .ValidateOnStart();

        services
            .AddOptions<CsrfOptions>()
            .BindConfiguration(CsrfOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHostedService<AuthCookieStartupValidator>();
        return services;
    }

    // --- API surface (controllers, versioning, swagger, validation) --------

    private static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new HeaderApiVersionReader("X-API-Version");
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
            });

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "TrackWallet API", Version = "v1" });

            options.DocInclusionPredicate(
                (documentName, apiDesc) =>
                {
                    if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo))
                        return true;

                    var versions = methodInfo
                        .DeclaringType?.GetCustomAttributes(typeof(ApiVersionAttribute), true)
                        .OfType<ApiVersionAttribute>()
                        .SelectMany(a => a.Versions);

                    return versions?.Any(v => $"v{v.MajorVersion}" == documentName) ?? true;
                }
            );
        });

        services.AddValidatorsFromAssemblyContaining<IRepository<Entity>>();

        return services;
    }

    // --- CORS --------------------------------------------------------------

    private static void BuildCorsPolicy(
        Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions options
    )
    {
        options.AddPolicy(
            CorsPolicyName,
            policy =>
                policy
                    .WithOrigins(
                        "http://localhost:3000",
                        "https://localhost:7067",
                        "https://localhost:4200"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
        );
    }

    // --- Forwarded headers, mapping, problem details, contexts -------------

    private static IServiceCollection ConfigureForwardedHeaders(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
            options.ForwardLimit = 1;
        });
        return services;
    }

    private static IServiceCollection AddObjectMapping(this IServiceCollection services)
    {
        TypeAdapterConfig config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(ServiceRegistration).Assembly);
        config.Scan(typeof(IRepository<>).Assembly);

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();
        return services;
    }

    private static IServiceCollection AddProblemDetailsPipeline(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
                context.ProblemDetails.Instance =
                    $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        });
        return services;
    }

    private static IServiceCollection AddRequestContexts(this IServiceCollection services)
    {
        services.AddScoped<IUserContext, HttpUserContext>();
        services.AddScoped<IFamilyContext, HttpFamilyContext>();
        return services;
    }
}
