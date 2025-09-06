using DemoDeck.Auth.Api.Services;
using DemoDeck.Auth.Api.Models;

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

            // HTTP Client for Tenant API calls
            builder.Services.AddHttpClient();

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
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
