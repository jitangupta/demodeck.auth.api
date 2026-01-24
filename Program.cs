using DemoDeck.Auth.Api.Services;
using DemoDeck.Auth.Api.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace DemoDeck.Auth.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configure CORS
            var corsAllowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:8080" };
            var corsAllowCredentials = builder.Configuration.GetValue<bool>("Cors:AllowCredentials", true);
            
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(corsAllowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                    
                    if (corsAllowCredentials)
                    {
                        policy.AllowCredentials();
                    }
                });
            });

            // HTTP Client for Tenant API calls
            builder.Services.AddHttpClient();

            // OpenTelemetry Tracing
            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService(serviceName: "auth-api", serviceVersion: "1.0.0")
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["deployment.environment"] = builder.Environment.EnvironmentName,
                        ["service.namespace"] = "demodeck"
                    }))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                        opts.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health");
                    })
                    .AddHttpClientInstrumentation(opts => opts.RecordException = true)
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri(builder.Configuration["OpenTelemetry:OtlpEndpoint"]
                            ?? "http://tempo.observability.svc.cluster.local:4317");
                    }));

            // JWT Configuration
            var jwtSettings = new JwtSettings
            {
                SecretKey = builder.Configuration["JwtSettings:SecretKey"] ?? "DemoDeckAuthSecretKey2024ForDevelopmentOnlyNeverUseInProduction",
                Issuer = builder.Configuration["JwtSettings:Issuer"] ?? "DemoDeckAuth",
                Audience = builder.Configuration["JwtSettings:Audience"] ?? "DemoDeckClients",
                TokenLifetimeMinutes = int.Parse(builder.Configuration["JwtSettings:TokenLifetimeMinutes"] ?? "60")
            };
            builder.Services.AddSingleton(jwtSettings);

            // Register services
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddSingleton<ITenantUserRepository, InMemoryTenantUserRepository>();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
