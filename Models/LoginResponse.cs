namespace DemoDeck.Auth.Api.Models
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserInfo? User { get; set; }
    }

    public class UserInfo
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
    }
}