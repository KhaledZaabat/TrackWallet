using Asp.Versioning;
using Expense_Tracker.App.Implemntation;
using Expense_Tracker.Application.Common.Behaivors;
using Expense_Tracker.Application.Common.Settings;
using Expense_Tracker.Application.Interfaces;
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
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
using System.Reflection;

namespace Expense_Tracker.App;



public static class ServiceRegistration
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {



        services
            .AddAssemblyScanningConfiguration()
            .AddInfrastructure(configuration)
            .RegisterOtpSettings()
            .AddAssemblyScanningConfiguration()
            .AddIdentityConfiguration()
            .AddJwtConfiguration(configuration)
            .AddControllersWithVersioning()
            .AddSwaggerDocs()
            .AddMediatRAndPipeline()
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


    private static IServiceCollection AddAssemblyScanningConfiguration(this IServiceCollection services)
    {


        services.Scan(scan => scan
            .FromAssembliesOf(typeof(AppDbContext))
            .AddClasses(c => c.AssignableTo<IScopedService>()).AsImplementedInterfaces().WithScopedLifetime()
            .AddClasses(c => c.AssignableTo<ITransientService>()).AsImplementedInterfaces().WithTransientLifetime()
            .AddClasses(c => c.AssignableTo<ISingletonService>()).AsImplementedInterfaces().WithSingletonLifetime()
        );

        return services;
    }

    private static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
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
     IConfiguration configuration)
    {
        services
            .AddOptions<JwtSettings>()
            .BindConfiguration(JwtSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<JwtSettings>>().Value);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

        services.ConfigureOptions<JwtBearerOptionsConfigurator>();

        services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"]
                    ?? throw new InvalidOperationException("Google ClientId is missing");

                options.ClientSecret = configuration["Authentication:Google:ClientSecret"]
                    ?? throw new InvalidOperationException("Google ClientSecret is missing");

                options.CallbackPath = "/signin-google";
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.SaveTokens = true;

                options.Scope.Add("email");
                options.Scope.Add("profile");

            });

        return services;
    }


    private static IServiceCollection AddControllersWithVersioning(this IServiceCollection services)
    {
        services.AddControllers();

        services.AddApiVersioning(options =>
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
        // Needed for minimal APIs / endpoint discovery
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TrackWallet API",
                Version = "v1"
            });


            options.DocInclusionPredicate((documentName, apiDesc) =>
            {
                if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo))
                    return true;

                var versions = methodInfo.DeclaringType?
                    .GetCustomAttributes(typeof(ApiVersionAttribute), true)
                    .OfType<ApiVersionAttribute>()
                    .SelectMany(a => a.Versions);


                return versions?.Any(v => $"v{v.MajorVersion}" == documentName) ?? true;
            });
        });

        return services;
    }


    private static IServiceCollection AddMediatRAndPipeline(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<IAppDbContext>();


            Mediator.LicenseKey = "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxNzk0MjY4ODAwIiwiaWF0IjoiMTc2MjczMjg3OCIsImFjY291bnRfaWQiOiIwMTlhNmIxMGUyZTc3NDNhYmE1ZmIyNjI5MDQ2ZDJkOCIsImN1c3RvbWVyX2lkIjoiY3RtXzAxazluaDJhczk5MXp0cjIybTljY2I5YnI4Iiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.EUJriLiiZ0vFJ0OTqGjUE2_FYtGiYvlOYIQCkwwGt8otj-n40PINMUxX1SHz2JxIDZcTkgtVSsNA2iLhzxvi1LzLCRSVXCLCxpOeWwv8EF3-sMkJoXLPlIhobVZV4iMKsGhRyMDYzjKJlSkdlx8OxRZQa23pX0a8IOouLgU1wwNSMyV10ASto3rd-8bK9zOGWJXA9QV5GO-GgykJSL_Qo9Q3cXprvdWQoxIC0DjrKHExfNCzgpEQ9qJWoo5gtahmwX7qsx0W1UPX0a5F5fyw6R5o4ANg5da9m-as0KacAw1wmM2_hIc1sfH4NcGmQ_dpoJ9qkqiBuaCFXxq_Szal3g";

        });

        return services;
    }

    private static IServiceCollection AddFluentValidationPipeline(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<IAppDbContext>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // model binding validation
        // services.AddFluentValidationAutoValidation();

        return services;
    }


    private static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
                policy
                    .WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });

        return services;
    }



    public static IServiceCollection AddInfrastructure(
     this IServiceCollection services,
     IConfiguration configuration)
    {

        services.AddScoped<UpdatableEntityInterceptor>();
        services.AddScoped<CreatableEntityInterceptor>();

        services.AddScoped<SoftDeleteEntityInterceptor>();

        //var connectionString = configuration.GetConnectionString("DefaultConnection");

        //services.AddDbContext<AppDbContext>(options =>
        //    options.UseSqlServer(connectionString));

        var connectionString = configuration.GetConnectionString("PostgreSqlConnection");
        services.AddDbContext<AppDbContext>((sp, options) =>
    options
    .UseNpgsql(connectionString)

        .AddInterceptors(
        sp.GetRequiredService<CreatableEntityInterceptor>(),
        sp.GetRequiredService<UpdatableEntityInterceptor>(),
        sp.GetRequiredService<SoftDeleteEntityInterceptor>())

        );






        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }

    public static IServiceCollection AddCache(
     this IServiceCollection services)
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
         IConfiguration configuration)
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

    private static IServiceCollection ConfigureBackGroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(Hangfireconfiguration => Hangfireconfiguration
         .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
         .UseSimpleAssemblyNameTypeSerializer()
         .UseRecommendedSerializerSettings()
         .UsePostgreSqlStorage(o => o.UseNpgsqlConnection(configuration.GetConnectionString("HangfirePostgreConnection"))));

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

        // Scan API and Application projects
        config.Scan(typeof(ServiceRegistration).Assembly);
        config.Scan(typeof(IAppDbContext).Assembly);

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }

    private static IServiceCollection ConfigureProblems(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance =
                    $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
            };
        }).AddProblemDetails();
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

        services.AddSingleton(sp =>
              sp.GetRequiredService<IOptions<OtpSettings>>().Value);
        return services;

    }


    private static IServiceCollection AddUrlBuilders(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();

        services.AddKeyedScoped<IUrlBuilder, FileUrlBuilder>("files", (provider, key) =>
        {
            var accessor = provider.GetRequiredService<IHttpContextAccessor>();

            HttpContext? httpContext = accessor.HttpContext;
            if (httpContext == null)
                throw new InvalidOperationException("IUrlBuilder cannot be created outside an HTTP request.");

            var factory = provider.GetRequiredService<IUrlHelperFactory>();

            var actionContext = new ActionContext(
                httpContext,
                httpContext.GetRouteData(),
                new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            );

            IUrlHelper urlHelper = factory.GetUrlHelper(actionContext);

            return new FileUrlBuilder(httpContext, urlHelper);
        });

        return services;
    }

}
