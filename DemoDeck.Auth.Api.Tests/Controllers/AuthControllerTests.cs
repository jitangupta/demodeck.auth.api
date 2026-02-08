using DemoDeck.Auth.Api.Controllers;
using DemoDeck.Auth.Api.Models;
using DemoDeck.Auth.Api.Services;
using DemoDeck.Auth.Api.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DemoDeck.Auth.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GenerateToken_InvalidModelState_ReturnsBadRequest()
    {
        _controller.ModelState.AddModelError("Username", "Required");
        var request = TestDataBuilder.CreateLoginRequest();

        var result = await _controller.GenerateToken(request);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequest.Value.Should().BeOfType<LoginResponse>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Invalid input data");
    }

    [Fact]
    public async Task GenerateToken_AuthSuccess_ReturnsOk()
    {
        var request = TestDataBuilder.CreateLoginRequest();
        var loginResponse = new LoginResponse
        {
            Success = true,
            Token = "test-token",
            Message = "Authentication successful",
            User = new UserInfo { Username = "testuser", Email = "test@example.com", Role = "User", TenantName = "test-tenant" }
        };
        _mockAuthService.Setup(s => s.AuthenticateAsync(request)).ReturnsAsync(loginResponse);

        var result = await _controller.GenerateToken(request);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<LoginResponse>().Subject;
        response.Success.Should().BeTrue();
        response.Token.Should().Be("test-token");
    }

    [Fact]
    public async Task GenerateToken_AuthFailure_ReturnsBadRequest()
    {
        var request = TestDataBuilder.CreateLoginRequest();
        var loginResponse = new LoginResponse { Success = false, Message = "Invalid credentials" };
        _mockAuthService.Setup(s => s.AuthenticateAsync(request)).ReturnsAsync(loginResponse);

        var result = await _controller.GenerateToken(request);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequest.Value.Should().BeOfType<LoginResponse>().Subject;
        response.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateToken_ValidRequest_CallsAuthServiceOnce()
    {
        var request = TestDataBuilder.CreateLoginRequest();
        _mockAuthService.Setup(s => s.AuthenticateAsync(request))
            .ReturnsAsync(new LoginResponse { Success = true });

        await _controller.GenerateToken(request);

        _mockAuthService.Verify(s => s.AuthenticateAsync(request), Times.Once);
    }

    [Fact]
    public async Task GenerateToken_AuthSuccess_LogsInformation()
    {
        var request = TestDataBuilder.CreateLoginRequest();
        _mockAuthService.Setup(s => s.AuthenticateAsync(request))
            .ReturnsAsync(new LoginResponse { Success = true });

        await _controller.GenerateToken(request);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
