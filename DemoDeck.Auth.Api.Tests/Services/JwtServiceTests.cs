using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DemoDeck.Auth.Api.Models;
using DemoDeck.Auth.Api.Services;
using DemoDeck.Auth.Api.Tests.Helpers;
using Microsoft.IdentityModel.Tokens;

namespace DemoDeck.Auth.Api.Tests.Services;

public class JwtServiceTests
{
    private readonly JwtSettings _settings;
    private readonly JwtService _service;
    private readonly TenantUser _user;
    private const string TenantName = "test-tenant";

    public JwtServiceTests()
    {
        _settings = TestDataBuilder.CreateJwtSettings();
        _service = new JwtService(_settings);
        _user = TestDataBuilder.CreateActiveUser();
    }

    private JwtSecurityToken DecodeToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        return handler.ReadJwtToken(token);
    }

    [Fact]
    public void GenerateToken_ReturnsNonEmptyString()
    {
        var token = _service.GenerateToken(_user, TenantName);

        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateToken_ReturnsValidJwtFormat()
    {
        var token = _service.GenerateToken(_user, TenantName);

        token.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public void GenerateToken_ContainsNameIdentifierClaim()
    {
        var token = _service.GenerateToken(_user, TenantName);
        var jwt = DecodeToken(token);

        // JwtSecurityTokenHandler maps ClaimTypes.NameIdentifier to "nameid"
        jwt.Claims.Should().Contain(c =>
            c.Type == "nameid" && c.Value == _user.Id.ToString());
    }

    [Fact]
    public void GenerateToken_ContainsNameClaim()
    {
        var token = _service.GenerateToken(_user, TenantName);
        var jwt = DecodeToken(token);

        // JwtSecurityToken maps ClaimTypes.Name to the "unique_name" claim type
        jwt.Claims.Should().Contain(c =>
            c.Type == JwtRegisteredClaimNames.UniqueName && c.Value == _user.Username);
    }

    [Fact]
    public void GenerateToken_ContainsEmailClaim()
    {
        var token = _service.GenerateToken(_user, TenantName);
        var jwt = DecodeToken(token);

        // JwtSecurityTokenHandler maps ClaimTypes.Email to "email"
        jwt.Claims.Should().Contain(c =>
            c.Type == "email" && c.Value == _user.Email);
    }

    [Fact]
    public void GenerateToken_ContainsTenantClaim()
    {
        var token = _service.GenerateToken(_user, TenantName);
        var jwt = DecodeToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "tenant" && c.Value == TenantName);
    }

    [Fact]
    public void GenerateToken_ContainsTenantIdClaim()
    {
        var token = _service.GenerateToken(_user, TenantName);
        var jwt = DecodeToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "tenant_id" && c.Value == _user.TenantId);
    }

    [Fact]
    public void GenerateToken_ContainsUserRoleClaim()
    {
        var token = _service.GenerateToken(_user, TenantName);
        var jwt = DecodeToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "user_role" && c.Value == _user.Role);
    }

    [Fact]
    public void GenerateToken_SetsIssuerToTenantName()
    {
        var token = _service.GenerateToken(_user, TenantName);
        var jwt = DecodeToken(token);

        jwt.Issuer.Should().Be(TenantName);
    }

    [Fact]
    public void GenerateToken_SetsAudienceToTenantName()
    {
        var token = _service.GenerateToken(_user, TenantName);
        var jwt = DecodeToken(token);

        jwt.Audiences.Should().Contain(TenantName);
    }

    [Fact]
    public void GenerateToken_ExpiresAtCorrectTime()
    {
        var before = DateTime.UtcNow.AddMinutes(_settings.TokenLifetimeMinutes);

        var token = _service.GenerateToken(_user, TenantName);

        var after = DateTime.UtcNow.AddMinutes(_settings.TokenLifetimeMinutes);
        var jwt = DecodeToken(token);
        jwt.ValidTo.Should().BeOnOrAfter(before.AddSeconds(-1)).And.BeOnOrBefore(after.AddSeconds(1));
    }

    [Fact]
    public void GenerateToken_UsesHmacSha256()
    {
        var token = _service.GenerateToken(_user, TenantName);
        var jwt = DecodeToken(token);

        jwt.Header.Alg.Should().Be("HS256");
    }
}
