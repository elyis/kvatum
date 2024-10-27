using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MimeDetective;
using MimeDetective.Definitions.Licensing;
using KVATUM_FILE_SERVICE.App.Service;
using KVATUM_FILE_SERVICE.Core.IService;
using KVATUM_FILE_SERVICE.Infrastructure.Service;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);


ConfigureServices(builder.Services);
var app = builder.Build();
app = ConfigureApplication(app);
app.Run();

string GetEnvVar(string name) => Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} is not set");
void ConfigureServices(IServiceCollection services)
{
    var rabbitMqHostname = GetEnvVar("RABBITMQ_HOSTNAME");
    var rabbitMqUsername = GetEnvVar("RABBITMQ_USERNAME");
    var rabbitMqPassword = GetEnvVar("RABBITMQ_PASSWORD");

    var rabbitMqProfileImageQueue = GetEnvVar("RABBITMQ_PROFILE_IMAGE_QUEUE_NAME");
    var rabbitMqHubIconQueue = GetEnvVar("RABBITMQ_HUB_ICON_QUEUE_NAME");
    var rabbitMqWorkspaceIconQueue = GetEnvVar("RABBITMQ_WORKSPACE_ICON_QUEUE_NAME");

    var jwtSecret = GetEnvVar("JWT_AUTH_SECRET");
    var jwtIssuer = GetEnvVar("JWT_AUTH_ISSUER");
    var jwtAudience = GetEnvVar("JWT_AUTH_AUDIENCE");
    var corsAllowedOrigins = GetEnvVar("CORS_ALLOWED_ORIGINS");

    var fileInspector = new ContentInspectorBuilder()
    {
        Definitions = new MimeDetective.Definitions.CondensedBuilder()
        {
            UsageType = UsageType.PersonalNonCommercial
        }.Build()
    }.Build();

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

    ConfigureJwtAuthentication(services, jwtSecret, jwtIssuer, jwtAudience);
    ConfigureSwagger(services);

    services.AddAuthorization();

    services.AddSingleton<IFileUploaderService, LocalFileUploaderService>();
    services.AddSingleton<IJwtService, JwtService>();
    services.AddSingleton<INotifyService, RabbitMqNotifyService>(sp =>
        new RabbitMqNotifyService(
            rabbitMqHostname,
            rabbitMqUsername,
            rabbitMqPassword,
            rabbitMqProfileImageQueue,
            rabbitMqHubIconQueue,
            rabbitMqWorkspaceIconQueue
        ));
    services.AddSingleton(fileInspector);
}

WebApplication ConfigureApplication(WebApplication app)
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
    app.UseAuthorization();
    app.MapControllers();
    app.MapGet("/", () => "File server work");

    return app;
}

void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "KVATUM_FILE_SERVICE_api",
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

void ConfigureJwtAuthentication(IServiceCollection services, string secret, string issuer, string audience)
{
    services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidIssuer = issuer,
            ValidAudience = audience
        });
}