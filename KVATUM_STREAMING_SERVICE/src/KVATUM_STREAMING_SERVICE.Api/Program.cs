using System.Text;
using KVATUM_STREAMING_SERVICE.App.Handler;
using KVATUM_STREAMING_SERVICE.App.Service;
using KVATUM_STREAMING_SERVICE.Core.IHandler;
using KVATUM_STREAMING_SERVICE.Core.IService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services);

var app = builder.Build();

ConfigureMiddleware(app);
app.MapGet("/", () => $"Streaming server work");
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.Zero,
});
app.UseRouting();

app.Run();

string GetEnvVar(string name) => Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} is not set");

void ConfigureServices(IServiceCollection services)
{
    var jwtSecret = GetEnvVar("JWT_AUTH_SECRET");
    var jwtIssuer = GetEnvVar("JWT_AUTH_ISSUER");
    var jwtAudience = GetEnvVar("JWT_AUTH_AUDIENCE");

    var corsAllowedOrigins = GetEnvVar("CORS_ALLOWED_ORIGINS");
    var redisConnectionString = GetEnvVar("REDIS_CONNECTION_STRING");

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

    var accountServiceUrl = GetEnvVar("AUTH_SERVICE_BASE_URL");
    services.AddHttpClient("AccountServiceClient", client =>
    {
        client.BaseAddress = new Uri(accountServiceUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    });

    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "kvatum";
    });

    services.AddDistributedMemoryCache();

    services.AddAuthorization();
    services.AddSingleton<IRoomConnectionService, RoomConnectionService>();
    services.AddSingleton<ISerializationService, SerializationService>();
    services.AddSingleton<IJwtService, JwtService>();
    services.AddSingleton<IMainConnectionService, MainConnectionService>();
    services.AddSingleton<IAccountService, AccountService>();

    services.AddSingleton<IEventHandler, HandleAnswer>();
    services.AddSingleton<IEventHandler, HandleChangeMicroState>();
    services.AddSingleton<IEventHandler, HandleChangeVideoState>();
    services.AddSingleton<IEventHandler, HandleDisconnect>();
    services.AddSingleton<IEventHandler, HandleIceCandidate>();
    services.AddSingleton<IEventHandler, HandleJoinToRoom>();
    services.AddSingleton<IEventHandler, HandleOffer>();
    services.AddSingleton<IEventHandler, HandleUpdateAnswer>();
    services.AddSingleton<IEventHandler, HandleUpdateOffer>();
    services.AddSingleton<IEventHandler, HandlePing>();
    services.AddSingleton<ICacheService, CacheService>();

    services.AddSingleton<IMessageHandler, MessageHandler>();

    services.AddScoped<IMainConnectionHandler, MainConnectionHandler>();

    ConfigureSwagger(services);
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
            Title = "Streaming service Api",
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