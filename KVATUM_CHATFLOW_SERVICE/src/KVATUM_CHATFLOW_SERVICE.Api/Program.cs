using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using KVATUM_CHATFLOW_SERVICE.App.Service;
using KVATUM_CHATFLOW_SERVICE.Core.IRepository;
using KVATUM_CHATFLOW_SERVICE.Core.IService;
using KVATUM_CHATFLOW_SERVICE.Infrastructure.Data;
using KVATUM_CHATFLOW_SERVICE.Infrastructure.Repository;
using Swashbuckle.AspNetCore.Filters;
using KVATUM_CHATFLOW_SERVICE.Infrastructure.Service;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services);

var app = builder.Build();

ConfigureMiddleware(app);
ApplyMigrations(app);

app.MapGet("/", () => $"ChatFlow works");
app.Run();



string GetEnvVar(string name) => Environment.GetEnvironmentVariable(name) ?? throw new Exception($"Environment variable {name} not found");

void ConfigureServices(IServiceCollection services)
{
    var chatFlowDbConnectionString = GetEnvVar("CHAT_FLOW_DB_CONNECTION_STRING");

    var corsAllowedOrigins = GetEnvVar("CORS_ALLOWED_ORIGINS");

    var jwtSecret = GetEnvVar("JWT_AUTH_SECRET");
    var jwtIssuer = GetEnvVar("JWT_AUTH_ISSUER");
    var jwtAudience = GetEnvVar("JWT_AUTH_AUDIENCE");

    var rabbitMqHostname = GetEnvVar("RABBITMQ_HOSTNAME");
    var rabbitMqUserName = GetEnvVar("RABBITMQ_USERNAME");
    var rabbitMqPassword = GetEnvVar("RABBITMQ_PASSWORD");
    var updateHubIconQueue = GetEnvVar("RABBITMQ_HUB_ICON_QUEUE_NAME");

    services.AddControllers(e =>
    {
        e.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
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
                .SetApplicationName("chatflow");

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

    services.AddDbContext<ServerFlowDbContext>(options =>
    {
        options.UseNpgsql(chatFlowDbConnectionString, builder =>
        {
            builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        });
    });

    services.AddSingleton<IHashGenerator, HashGenerator>();
    services.AddSingleton<IJwtService, JwtService>();

    services.AddSingleton<RabbitMqService>(provider =>
        {
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
            return new RabbitMqService(
                scopeFactory,
                rabbitMqHostname,
                rabbitMqUserName,
                rabbitMqPassword,
                updateHubIconQueue
            );
        });


    services.AddScoped<IHubRepository, HubRepository>();
    services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
    services.AddScoped<IChatRepository, ChatRepository>();

    services.AddScoped<IChatService, ChatService>();
    services.AddScoped<IWorkspaceService, WorkspaceService>();
    services.AddScoped<IHubService, HubService>();

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
        var context = services.GetRequiredService<ServerFlowDbContext>();
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
            Title = "ChatFlow Api",
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