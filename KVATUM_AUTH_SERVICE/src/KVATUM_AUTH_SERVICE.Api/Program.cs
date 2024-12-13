using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using KVATUM_AUTH_SERVICE.Infrastructure.Data;
using KVATUM_AUTH_SERVICE.Infrastructure.Repository;
using KVATUM_AUTH_SERVICE.Infrastructure.Service;
using Swashbuckle.AspNetCore.Filters;
using KVATUM_AUTH_SERVICE.Core.IService;
using KVATUM_AUTH_SERVICE.App.Service;
using KVATUM_AUTH_SERVICE.Core.IRepository;


var builder = WebApplication.CreateBuilder(args);

var host = GetEnvVar("HOST");
builder.WebHost.UseUrls(host);
builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(8888, listenOptions =>
    {
        listenOptions.UseHttps("/https/local.pfx", "*Scores");
    });
});

ConfigureServices(builder.Services);

var app = builder.Build();

ConfigureMiddleware(app);
ApplyMigrations(app);

app.MapGet("/", () => $"Auth server work");

app.Run();



string GetEnvVar(string name) => Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} is not set");

void ConfigureServices(IServiceCollection services)
{
    var authDbConnectionString = GetEnvVar("AUTH_DB_CONNECTION_STRING");
    var passwordHashKey = GetEnvVar("PASSWORD_HASH_KEY");
    var corsAllowedOrigins = GetEnvVar("CORS_ALLOWED_ORIGINS");

    var jwtSecret = GetEnvVar("JWT_AUTH_SECRET");
    var jwtIssuer = GetEnvVar("JWT_AUTH_ISSUER");
    var jwtAudience = GetEnvVar("JWT_AUTH_AUDIENCE");

    var rabbitMqHostname = GetEnvVar("RABBITMQ_HOSTNAME");
    var rabbitMqUserName = GetEnvVar("RABBITMQ_USERNAME");
    var rabbitMqPassword = GetEnvVar("RABBITMQ_PASSWORD");

    var profileImageQueueName = GetEnvVar("RABBITMQ_PROFILE_IMAGE_QUEUE_NAME");
    var sendConfirmationCodeQueueName = GetEnvVar("RABBITMQ_SEND_CONFIRMATION_CODE_QUEUE_NAME");

    var redisConnectionString = GetEnvVar("REDIS_CONNECTION_STRING");
    var redisInstanceName = GetEnvVar("REDIS_INSTANCE_NAME");

    services.AddControllers(e =>
    {
        e.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
    });

    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = redisInstanceName;
    });

    services.AddCors(setup =>
    {
        setup.AddDefaultPolicy(options =>
        {
            options.AllowAnyHeader();
            options.WithOrigins(corsAllowedOrigins.Split(","));
            options.AllowAnyMethod();
        });
    });

    services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo("/keys"))
                .SetApplicationName("KVATUM_AUTH_SERVICE");

    services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience
        });

    services.AddDistributedMemoryCache();
    services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(15);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    services.AddAuthorization();

    ConfigureSwagger(services);

    services.AddDbContext<AuthDbContext>(options =>
    {
        options.UseNpgsql(authDbConnectionString, builder =>
        {
            builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        });
        options.LogTo(Console.WriteLine, LogLevel.Warning);
    });

    services.AddSingleton<IHashPasswordService, HashPasswordService>(sp => new HashPasswordService(passwordHashKey));
    services.AddSingleton<RabbitMqService>(provider =>
        {
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
            return new RabbitMqService(
                scopeFactory,
                rabbitMqHostname,
                rabbitMqUserName,
                rabbitMqPassword,
                profileImageQueueName
            );
        });
    services.AddScoped<IAccountService, AccountService>();

    services.AddSingleton<INotifyService, RabbitMqNotifyService>(sp => new RabbitMqNotifyService(
        rabbitMqHostname,
        rabbitMqUserName,
        rabbitMqPassword,
        sendConfirmationCodeQueueName
    ));

    services.AddSingleton<IJwtService, JwtService>(sp => new JwtService(
        jwtSecret,
        jwtIssuer,
        jwtAudience));


    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IAccountSessionService, AccountSessionService>();
    services.AddScoped<IAccountRepository, AccountRepository>();
    services.AddScoped<IUnverifiedAccountRepository, UnverifiedAccountRepository>();
    services.AddScoped<ISessionRepository, SessionRepository>();

    services.AddSingleton<ICacheService, CacheService>();

    services.AddHostedService(provider => provider.GetRequiredService<RabbitMqService>());
}

void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRouting();
    app.UseCors();

    app.UseAuthentication();
    app.UseSession();
    app.UseAuthorization();

    app.MapControllers();
}

void ApplyMigrations(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AuthDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "KVATUM Auth Api",
            Description = "Api",
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Bearer auth scheme",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        });

        options.OperationFilter<SecurityRequirementsOperationFilter>();

        options.EnableAnnotations();
    });
}