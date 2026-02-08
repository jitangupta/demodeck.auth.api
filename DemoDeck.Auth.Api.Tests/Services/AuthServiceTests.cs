using System.Net;
using DemoDeck.Auth.Api.Models;
using DemoDeck.Auth.Api.Services;
using DemoDeck.Auth.Api.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DemoDeck.Auth.Api.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<ITenantUserRepository> _mockRepo;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfig;

    public AuthServiceTests()
    {
        _mockRepo = new Mock<ITenantUserRepository>();
        _mockJwtService = new Mock<IJwtService>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        _mockConfig = TestDataBuilder.CreateMockConfiguration();
    }

    private AuthService CreateService(MockHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);
        var mockFactory = new Mock<IHttpClientFactory>();
        mockFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
        return new AuthService(_mockRepo.Object, _mockJwtService.Object, mockFactory.Object, _mockLogger.Object, _mockConfig.Object);
    }

    private AuthService CreateServiceWithSuccessfulTenant(string tenantId = "tnt_test001", string tenantName = "test-tenant")
    {
        var json = TestDataBuilder.CreateSuccessfulTenantApiResponse(tenantId, tenantName);
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        return CreateService(handler);
    }

    private void SetupUserFound(TenantUser user)
    {
        _mockRepo.Setup(r => r.GetUserByUsernameAsync(user.Username, user.TenantId))
            .ReturnsAsync(user);
    }

    // --- AuthenticateAsync: Success Path ---

    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsSuccess()
    {
        var user = TestDataBuilder.CreateActiveUser();
        SetupUserFound(user);
        _mockJwtService.Setup(j => j.GenerateToken(user, "test-tenant")).Returns("generated-token");
        _mockRepo.Setup(r => r.UpdateUserAsync(It.IsAny<TenantUser>())).ReturnsAsync(true);
        var service = CreateServiceWithSuccessfulTenant();
        var request = TestDataBuilder.CreateLoginRequest(username: user.Username);

        var result = await service.AuthenticateAsync(request);

        result.Success.Should().BeTrue();
        result.Token.Should().Be("generated-token");
        result.Message.Should().Be("Authentication successful");
        result.User.Should().NotBeNull();
        result.User!.Username.Should().Be(user.Username);
        result.User.Email.Should().Be(user.Email);
        result.User.Role.Should().Be(user.Role);
        result.User.TenantName.Should().Be("test-tenant");
    }

    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_UpdatesLastLogin()
    {
        var user = TestDataBuilder.CreateActiveUser();
        SetupUserFound(user);
        _mockJwtService.Setup(j => j.GenerateToken(user, "test-tenant")).Returns("token");
        _mockRepo.Setup(r => r.UpdateUserAsync(It.IsAny<TenantUser>())).ReturnsAsync(true);
        var service = CreateServiceWithSuccessfulTenant();
        var request = TestDataBuilder.CreateLoginRequest(username: user.Username);

        await service.AuthenticateAsync(request);

        _mockRepo.Verify(r => r.UpdateUserAsync(It.Is<TenantUser>(u =>
            u.LastLoginAt != null && u.LastLoginAt.Value >= DateTime.UtcNow.AddSeconds(-5))), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ExpiresInOneHour()
    {
        var user = TestDataBuilder.CreateActiveUser();
        SetupUserFound(user);
        _mockJwtService.Setup(j => j.GenerateToken(user, "test-tenant")).Returns("token");
        _mockRepo.Setup(r => r.UpdateUserAsync(It.IsAny<TenantUser>())).ReturnsAsync(true);
        var service = CreateServiceWithSuccessfulTenant();
        var request = TestDataBuilder.CreateLoginRequest(username: user.Username);
        var before = DateTime.UtcNow.AddMinutes(60);

        var result = await service.AuthenticateAsync(request);

        var after = DateTime.UtcNow.AddMinutes(60);
        result.ExpiresAt.Should().BeOnOrAfter(before.AddSeconds(-1)).And.BeOnOrBefore(after.AddSeconds(1));
    }

    // --- AuthenticateAsync: Tenant Validation Failures ---

    [Fact]
    public async Task AuthenticateAsync_TenantApiReturns404_ReturnsInvalidTenant()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "");
        var service = CreateService(handler);
        var request = TestDataBuilder.CreateLoginRequest();

        var result = await service.AuthenticateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid tenant");
    }

    [Fact]
    public async Task AuthenticateAsync_TenantApiReturnsSuccessFalse_ReturnsInvalidTenant()
    {
        var json = TestDataBuilder.CreateFailedTenantApiResponse();
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var service = CreateService(handler);
        var request = TestDataBuilder.CreateLoginRequest();

        var result = await service.AuthenticateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid tenant");
    }

    [Fact]
    public async Task AuthenticateAsync_TenantApiThrowsException_ReturnsInvalidTenant()
    {
        var handler = new MockHttpMessageHandler(new HttpRequestException("Connection refused"));
        var service = CreateService(handler);
        var request = TestDataBuilder.CreateLoginRequest();

        var result = await service.AuthenticateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid tenant");
    }

    // --- AuthenticateAsync: User/Password Failures ---

    [Fact]
    public async Task AuthenticateAsync_UserNotFound_ReturnsInvalidCredentials()
    {
        _mockRepo.Setup(r => r.GetUserByUsernameAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((TenantUser?)null);
        var service = CreateServiceWithSuccessfulTenant();
        var request = TestDataBuilder.CreateLoginRequest();

        var result = await service.AuthenticateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task AuthenticateAsync_InactiveUser_ReturnsInvalidCredentials()
    {
        var user = TestDataBuilder.CreateInactiveUser(username: "testuser", tenantId: "tnt_test001");
        _mockRepo.Setup(r => r.GetUserByUsernameAsync("testuser", "tnt_test001"))
            .ReturnsAsync(user);
        var service = CreateServiceWithSuccessfulTenant();
        var request = TestDataBuilder.CreateLoginRequest(username: "testuser");

        var result = await service.AuthenticateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task AuthenticateAsync_WrongPassword_ReturnsInvalidCredentials()
    {
        var user = TestDataBuilder.CreateActiveUser();
        SetupUserFound(user);
        var service = CreateServiceWithSuccessfulTenant();
        var request = TestDataBuilder.CreateLoginRequest(username: user.Username, password: "wrongpassword");

        var result = await service.AuthenticateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid credentials");
    }

    // --- AuthenticateAsync: Exception Handling ---

    [Fact]
    public async Task AuthenticateAsync_JwtServiceThrows_ReturnsAuthFailed()
    {
        var user = TestDataBuilder.CreateActiveUser();
        SetupUserFound(user);
        _mockJwtService.Setup(j => j.GenerateToken(It.IsAny<TenantUser>(), It.IsAny<string>()))
            .Throws(new Exception("JWT generation error"));
        var service = CreateServiceWithSuccessfulTenant();
        var request = TestDataBuilder.CreateLoginRequest(username: user.Username);

        var result = await service.AuthenticateAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Authentication failed");
    }

    [Fact]
    public async Task AuthenticateAsync_ExceptionOccurs_LogsError()
    {
        var user = TestDataBuilder.CreateActiveUser();
        SetupUserFound(user);
        _mockJwtService.Setup(j => j.GenerateToken(It.IsAny<TenantUser>(), It.IsAny<string>()))
            .Throws(new Exception("JWT error"));
        var service = CreateServiceWithSuccessfulTenant();
        var request = TestDataBuilder.CreateLoginRequest(username: user.Username);

        await service.AuthenticateAsync(request);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // --- ValidatePasswordAsync ---

    [Fact]
    public async Task ValidatePassword_CorrectPassword_ReturnsTrue()
    {
        var user = TestDataBuilder.CreateActiveUser();
        var service = CreateServiceWithSuccessfulTenant();

        var result = await service.ValidatePasswordAsync(user, "password123");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatePassword_WrongPassword_ReturnsFalse()
    {
        var user = TestDataBuilder.CreateActiveUser();
        var service = CreateServiceWithSuccessfulTenant();

        var result = await service.ValidatePasswordAsync(user, "wrongpassword");

        result.Should().BeFalse();
    }

    // --- GetUserByUsernameAsync ---

    [Fact]
    public async Task GetUserByUsername_DelegatesToRepository()
    {
        var expectedUser = TestDataBuilder.CreateActiveUser();
        _mockRepo.Setup(r => r.GetUserByUsernameAsync("testuser", "tnt_test001"))
            .ReturnsAsync(expectedUser);
        var service = CreateServiceWithSuccessfulTenant();

        var result = await service.GetUserByUsernameAsync("testuser", "tnt_test001");

        result.Should().BeSameAs(expectedUser);
        _mockRepo.Verify(r => r.GetUserByUsernameAsync("testuser", "tnt_test001"), Times.Once);
    }
}
