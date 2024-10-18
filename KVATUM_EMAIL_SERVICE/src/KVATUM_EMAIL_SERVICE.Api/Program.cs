using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using KVATUM_EMAIL_SERVICE.App.Service;
using KVATUM_EMAIL_SERVICE.Core.IService;
using KVATUM_EMAIL_SERVICE.Infrastructure.Service;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services);

var app = builder.Build();

ConfigureMiddleware(app);
app.MapGet("/", () => $"Email server work");

app.Run();

string GetEnvVar(string name) => Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} is not set");

void ConfigureServices(IServiceCollection services)
{
    var emailSenderName = GetEnvVar("EMAIL_SENDER_NAME");
    var emailSenderEmail = GetEnvVar("EMAIL_SENDER_EMAIL");
    var emailSmtpServer = GetEnvVar("EMAIL_SMTP_SERVER");
    var emailSmtpPort = int.Parse(GetEnvVar("EMAIL_SMTP_PORT"));
    var emailSenderPassword = GetEnvVar("EMAIL_SENDER_PASSWORD");

    var jwtSecret = GetEnvVar("JWT_AUTH_SECRET");
    var jwtIssuer = GetEnvVar("JWT_AUTH_ISSUER");
    var jwtAudience = GetEnvVar("JWT_AUTH_AUDIENCE");

    var corsAllowedOrigins = GetEnvVar("CORS_ALLOWED_ORIGINS");
    var rabbitMqHostname = GetEnvVar("RABBITMQ_HOSTNAME");
    var rabbitMqUserName = GetEnvVar("RABBITMQ_USERNAME");
    var rabbitMqPassword = GetEnvVar("RABBITMQ_PASSWORD");
    var sendConfirmationCodeQueueName = GetEnvVar("RABBITMQ_SEND_CONFIRMATION_CODE_QUEUE_NAME");

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience
        });

    services.AddAuthorization();

    ConfigureSwagger(services);


    services.AddSingleton<IEmailService, EmailService>(sp => new EmailService(
        emailSenderEmail,
        emailSenderPassword,
        emailSenderName,
        emailSmtpServer,
        emailSmtpPort,
        sp.GetRequiredService<ILogger<EmailService>>()));

    services.AddSingleton<IJwtService, JwtService>();

    services.AddSingleton<RabbitMqService>(provider =>
        {
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
            return new RabbitMqService(
                scopeFactory,
                provider.GetRequiredService<IEmailService>(),
                rabbitMqHostname,
                rabbitMqUserName,
                rabbitMqPassword,
                sendConfirmationCodeQueueName
            );
        });

    services.AddSingleton<INotifyService, RabbitMqNotifyService>(sp => new RabbitMqNotifyService(
        rabbitMqHostname,
        rabbitMqUserName,
        rabbitMqPassword
    ));

    services.AddHostedService(provider => provider.GetRequiredService<RabbitMqService>());
}

void ConfigureMiddleware(WebApplication app)
{
    app.UseCors();
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
}

void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Planner email service Api",
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