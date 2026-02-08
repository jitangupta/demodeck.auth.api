using DemoDeck.Auth.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DemoDeck.Auth.Api.Tests.Controllers;

public class HealthControllerTests
{
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        _controller = new HealthController();
    }

    [Fact]
    public void Get_ReturnsOkResult()
    {
        var result = _controller.Get();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void Get_ReturnsHealthyStatus()
    {
        var result = _controller.Get() as OkObjectResult;

        var value = result!.Value!;
        var status = value.GetType().GetProperty("status")!.GetValue(value) as string;
        status.Should().Be("Healthy");
    }

    [Fact]
    public void Get_ReturnsCorrectServiceName()
    {
        var result = _controller.Get() as OkObjectResult;

        var value = result!.Value!;
        var service = value.GetType().GetProperty("service")!.GetValue(value) as string;
        service.Should().Be("DemoDeck.Auth.Api");
    }

    [Fact]
    public void Get_ReturnsCorrectVersion()
    {
        var result = _controller.Get() as OkObjectResult;

        var value = result!.Value!;
        var version = value.GetType().GetProperty("version")!.GetValue(value) as string;
        version.Should().Be("1.0.0");
    }

    [Fact]
    public void Get_ReturnsRecentTimestamp()
    {
        var before = DateTime.UtcNow;

        var result = _controller.Get() as OkObjectResult;

        var after = DateTime.UtcNow;
        var value = result!.Value!;
        var timestamp = (DateTime)value.GetType().GetProperty("timestamp")!.GetValue(value)!;
        timestamp.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }
}
