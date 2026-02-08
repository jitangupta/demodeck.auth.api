using System.Text.Json;
using DemoDeck.Auth.Api.Models;
using Microsoft.Extensions.Configuration;

namespace DemoDeck.Auth.Api.Tests.Helpers;

public static class TestDataBuilder
{
    // Pre-compute a single BCrypt hash for "password123" to avoid repeated hashing in tests
    private static readonly string Password123Hash = BCrypt.Net.BCrypt.HashPassword("password123");

    public static TenantUser CreateActiveUser(
        int id = 1,
        string username = "testuser",
        string email = "test@example.com",
        string tenantId = "tnt_test001",
        string role = "User")
    {
        return new TenantUser
        {
            Id = id,
            Username = username,
            Email = email,
            PasswordHash = Password123Hash,
            TenantId = tenantId,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
    }

    public static TenantUser CreateInactiveUser(
        int id = 2,
        string username = "inactive",
        string tenantId = "tnt_test001")
    {
        return new TenantUser
        {
            Id = id,
            Username = username,
            Email = "inactive@example.com",
            PasswordHash = Password123Hash,
            TenantId = tenantId,
            Role = "User",
            IsActive = false,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
    }

    public static LoginRequest CreateLoginRequest(
        string username = "testuser",
        string password = "password123",
        string tenantName = "test-tenant")
    {
        return new LoginRequest
        {
            Username = username,
            Password = password,
            TenantName = tenantName
        };
    }

    public static JwtSettings CreateJwtSettings(
        string secretKey = "ThisIsATestSecretKeyThatMustBeAtLeast32BytesLong!",
        int tokenLifetimeMinutes = 60)
    {
        return new JwtSettings
        {
            SecretKey = secretKey,
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            TokenLifetimeMinutes = tokenLifetimeMinutes
        };
    }

    public static string CreateSuccessfulTenantApiResponse(
        string tenantId = "tnt_test001",
        string tenantName = "test-tenant")
    {
        var response = new
        {
            Success = true,
            Data = new
            {
                TenantId = tenantId,
                TenantName = tenantName,
                DisplayName = "Test Tenant",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                PrimaryRegion = "us-east-1",
                CurrentVersion = "1.0.0",
                ServiceName = "test-service",
                IsHealthy = true,
                CustomHeaders = new Dictionary<string, string>()
            },
            Message = "",
            ErrorCode = ""
        };
        return JsonSerializer.Serialize(response);
    }

    public static string CreateFailedTenantApiResponse()
    {
        var response = new
        {
            Success = false,
            Data = (object?)null,
            Message = "Tenant not found",
            ErrorCode = "TENANT_NOT_FOUND"
        };
        return JsonSerializer.Serialize(response);
    }

    public static Mock<IConfiguration> CreateMockConfiguration(
        string tenantUrl = "https://api.example.com/api/tenant")
    {
        var mockConfig = new Mock<IConfiguration>();
        var mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(s => s.Value).Returns(tenantUrl);
        mockConfig.Setup(c => c.GetSection("TenantUrl")).Returns(mockSection.Object);
        mockConfig.Setup(c => c["TenantUrl"]).Returns(tenantUrl);
        return mockConfig;
    }
}
