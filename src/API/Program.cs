using Amazon.Extensions.NETCore.Setup;
using API.Filters;
using API.HealthChecks;
using API.Middlewares;
using Application;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Formatting.Compact;
using System.Text;



var builder = WebApplication.CreateBuilder(args);


var awsOptions = builder.Configuration.GetAWSOptions();
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<Amazon.S3.IAmazonS3>();
builder.Services.AddAWSService<Amazon.SQS.IAmazonSQS>();


#region Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();

builder.Host.UseSerilog();
#endregion

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ExceptionFilter>();
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hacka IADT SOAT - Serviço API",
        Version = "v1",
        Description = "Microserviço de Analise Diagreamas - IADT-SOAT"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não configurada.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ms-cadastros";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "mecanicaos";

var isTestEnvironment = builder.Environment.EnvironmentName == "Testing";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = !isTestEnvironment,
            ValidateAudience = !isTestEnvironment,
            ValidateLifetime = !isTestEnvironment,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = isTestEnvironment ? TimeSpan.FromDays(365) : TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddApplication();

builder.Services.AddInfrastructure(builder.Configuration, skipDbContext: isTestEnvironment, skipMassTransit: isTestEnvironment);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CorrelationIdMiddleware>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

if (!isTestEnvironment)
{
    
    // Adiciona o serviço de Health Checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);


    //var brokerProvider = builder.Configuration["MessageBroker:Provider"];

    //if (brokerProvider != "SQS")
    //{
    //    var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq-service";
    //    var rabbitUser = builder.Configuration["RabbitMQ:User"] ?? "guest";
    //    var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? "guest";

    //    hcBuilder.AddRabbitMQ(
    //        rabbitConnectionString: $"amqp://{rabbitUser}:{rabbitPass}@{rabbitHost}:5672",
    //        name: "rabbitmq",
    //        failureStatus: HealthStatus.Unhealthy,
    //        tags: new[] { "messaging", "rabbitmq", "ready" });
    //}
}

var app = builder.Build();

app.UseSwagger(c =>
{
    c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
    {
        //swaggerDoc.Servers = [
        //    new() { Url = "/api/ordens" }
        //];
    });
});

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("swagger/v1/swagger.json", "Hacka IADT SOAT v1");
    options.RoutePrefix = string.Empty;
});

app.UseCors("AllowAll");
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<OrdensDbContext>();
        db.Database.Migrate();
    }
}

app.MapControllers();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = HealthCheckResponseWriter.WriteResponse
    });

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false // Liveness: apenas verifica se a aplicação está rodando
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = HealthCheckResponseWriter.WriteResponse
    });
}

app.Run();

public partial class Program { }