using System.Security.Claims;
using DemoDeck.Auth.Api.Models;

namespace DemoDeck.Auth.Api.Services
{
    public interface IJwtService
    {
        string GenerateToken(TenantUser user, string tenantName);
        ClaimsPrincipal? ValidateToken(string token);
    }
}