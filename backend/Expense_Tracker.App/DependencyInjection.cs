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
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Resend;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Expense_Tracker.App;

public static class ServiceRegistration
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddAssemblyScanningConfiguration()
            .AddInfrastructure(configuration)
            .RegisterOtpSettings()
            .AddAssemblyScanningConfiguration()
            .AddIdentityConfiguration()
            .AddJwtConfiguration(configuration)
            .AddCookieAuthConfiguration()
            .AddControllersWithVersioning()
            .AddSwaggerDocs()
            .AddFluentValidationPipeline()
            .AddCorsPolicy()
            .AddCache()
            .AddMessageSending(configuration)
            .ConfigureBackGroundJobs(configuration)
            .ConfigureForwardedHeaders()
            .ConfigureMappings()
            .ConfigureProblems()
            .AddUserContext()
            .AddUrlBuilders()
            .AddFamilyContext();

        return services;
    }

    private static IServiceCollection AddAssemblyScanningConfiguration(
        this IServiceCollection services
    )
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

    private static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
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

        return services;
    }

    private static IServiceCollection AddJwtConfiguration(
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

    private static IServiceCollection AddCookieAuthConfiguration(this IServiceCollection services)
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

        // R22.8 — fail fast on invalid auth cookie configuration in non-Development environments.
        services.AddHostedService<AuthCookieStartupValidator>();

        return services;
    }

    private static IServiceCollection AddControllersWithVersioning(this IServiceCollection services)
    {
        services.AddControllers();

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

        return services;
    }

    public static IServiceCollection AddSwaggerDocs(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

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

        return services;
    }

    private static IServiceCollection AddFluentValidationPipeline(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<IRepository<Entity>>();

        return services;
    }

    private static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowFrontend",
                policy =>
                    policy
                        .WithOrigins("http://localhost:3000", "https://localhost:7067")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
            );
        });

        return services;
    }

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddScoped<UpdatableEntityInterceptor>();
        services.AddScoped<CreatableEntityInterceptor>();

        services.AddScoped<SoftDeleteEntityInterceptor>();

        //var connectionString = configuration.GetConnectionString("DefaultConnection");

        //services.AddDbContext<AppDbContext>(options =>
        //    options.UseSqlServer(connectionString));

        var connectionString = configuration.GetConnectionString("PostgreSqlConnection");
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

    public static IServiceCollection AddCache(this IServiceCollection services)
    {
        services.AddMemoryCache();

        return services;
    }

    //public static IServiceCollection AddMessageSending(
    // this IServiceCollection services,
    //    IConfiguration configuration)
    //{
    //    services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

    //    services.AddScoped<IEmailSender, EmailSender>();

    //    return services;

    //}

    public static IServiceCollection AddMessageSending(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<ResendSettings>(configuration.GetSection("Resend"));

        services.AddOptions();
        services.AddHttpClient<ResendClient>();

        services.Configure<ResendClientOptions>(o =>
        {
            o.ApiToken = configuration["Resend:ApiKey"]!;
        });

        services.AddTransient<IResend, ResendClient>();

        services.AddScoped<IEmailSender, ResendEmailSender>();

        return services;
    }

    private static IServiceCollection ConfigureBackGroundJobs(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHangfire(Hangfireconfiguration =>
            Hangfireconfiguration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(o =>
                    o.UseNpgsqlConnection(
                        configuration.GetConnectionString("HangfirePostgreConnection")
                    )
                )
        );

        //  .UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection")));

        // Add the processing server as IHostedService
        services.AddHangfireServer();
        return services;
    }

    private static IServiceCollection ConfigureForwardedHeaders(this IServiceCollection services)
    {
        _ = services.Configure<ForwardedHeadersOptions>(static options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            // Remove default restrictions so IIS/ARR/Cloudflare/etc. are allowed
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();

            // Prevent spoofed X-Forwarded-For values
            options.ForwardLimit = 1; // only trust 1 proxy hop
        });
        return services;
    }

    private static IServiceCollection ConfigureMappings(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;

        config.Scan(typeof(ServiceRegistration).Assembly);
        config.Scan(typeof(IRepository<>).Assembly);

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }

    private static IServiceCollection ConfigureProblems(this IServiceCollection services)
    {
        services
            .AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Instance =
                        $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                };
            })
            .AddProblemDetails();
        return services;
    }

    private static IServiceCollection AddUserContext(this IServiceCollection services)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<IUserContext, HttpUserContext>();

        return services;
    }

    private static IServiceCollection AddFamilyContext(this IServiceCollection services)
    {
        services.AddScoped<IFamilyContext, HttpFamilyContext>();

        return services;
    }

    private static IServiceCollection RegisterOtpSettings(this IServiceCollection services)
    {
        services
            .AddOptions<OtpSettings>()
            .BindConfiguration(OtpSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(sp => sp.GetRequiredService<IOptions<OtpSettings>>().Value);
        return services;
    }

    private static IServiceCollection AddUrlBuilders(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();

        services.AddKeyedScoped<IUrlBuilder, FileUrlBuilder>(
            "files",
            (provider, key) =>
            {
                var accessor = provider.GetRequiredService<IHttpContextAccessor>();

                HttpContext? httpContext = accessor.HttpContext;
                if (httpContext == null)
                    throw new InvalidOperationException(
                        "IUrlBuilder cannot be created outside an HTTP request."
                    );

                var factory = provider.GetRequiredService<IUrlHelperFactory>();

                var actionContext = new ActionContext(
                    httpContext,
                    httpContext.GetRouteData(),
                    new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
                );

                IUrlHelper urlHelper = factory.GetUrlHelper(actionContext);

                return new FileUrlBuilder(httpContext, urlHelper);
            }
        );

        services.AddScoped<IUrlBuilder>(sp => sp.GetRequiredKeyedService<IUrlBuilder>("files"));

        return services;
    }
}
