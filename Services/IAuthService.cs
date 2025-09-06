using DemoDeck.Auth.Api.Models;

namespace DemoDeck.Auth.Api.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> AuthenticateAsync(LoginRequest request);
        Task<TenantUser?> GetUserByUsernameAsync(string username, string tenantId);
        Task<bool> ValidatePasswordAsync(TenantUser user, string password);
    }
}