using DemoDeck.Auth.Api.Models;

namespace DemoDeck.Auth.Api.Services
{
    public interface ITenantUserRepository
    {
        Task<TenantUser?> GetUserByUsernameAsync(string username, string tenantId);
        Task<TenantUser?> GetUserByIdAsync(int userId);
        Task<bool> CreateUserAsync(TenantUser user);
        Task<bool> UpdateUserAsync(TenantUser user);
        Task<List<TenantUser>> GetUsersByTenantAsync(string tenantId);
    }
}