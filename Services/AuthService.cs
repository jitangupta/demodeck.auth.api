using System.Text.Json;
using DemoDeck.Auth.Api.Models;

namespace DemoDeck.Auth.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITenantUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ITenantUserRepository userRepository,
            IJwtService jwtService,
            IHttpClientFactory httpClientFactory,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<LoginResponse> AuthenticateAsync(LoginRequest request)
        {
            try
            {
                // 1. Validate tenant exists via Tenant API
                var tenantInfo = await ValidateTenantAsync(request.TenantName);
                if (tenantInfo == null)
                {
                    return new LoginResponse 
                    { 
                        Success = false, 
                        Message = "Invalid tenant" 
                    };
                }

                // 2. Find user in tenant
                var user = await GetUserByUsernameAsync(request.Username, tenantInfo.TenantId);
                if (user == null || !user.IsActive)
                {
                    return new LoginResponse 
                    { 
                        Success = false, 
                        Message = "Invalid credentials" 
                    };
                }

                // 3. Validate password
                if (!await ValidatePasswordAsync(user, request.Password))
                {
                    return new LoginResponse 
                    { 
                        Success = false, 
                        Message = "Invalid credentials" 
                    };
                }

                // 4. Generate JWT token
                var token = _jwtService.GenerateToken(user, request.TenantName);
                var expiresAt = DateTime.UtcNow.AddMinutes(60);

                // 5. Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);

                return new LoginResponse
                {
                    Success = true,
                    Token = token,
                    ExpiresAt = expiresAt,
                    Message = "Authentication successful",
                    User = new UserInfo
                    {
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role,
                        TenantName = request.TenantName
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication failed for user {Username} in tenant {TenantName}", 
                    request.Username, request.TenantName);
                
                return new LoginResponse 
                { 
                    Success = false, 
                    Message = "Authentication failed" 
                };
            }
        }

        private async Task<TenantInfo?> ValidateTenantAsync(string tenantName)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"https://localhost:5001/api/tenant/{tenantName}");
                
                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<TenantDto>>(content, options);
                
                return apiResponse?.Success == true && apiResponse.Data != null
                    ? new TenantInfo { TenantId = apiResponse.Data.TenantId, TenantName = tenantName }
                    : null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to validate tenant {TenantName}", tenantName);
                return null;
            }
        }

        public async Task<TenantUser?> GetUserByUsernameAsync(string username, string tenantId)
        {
            return await _userRepository.GetUserByUsernameAsync(username, tenantId);
        }

        public async Task<bool> ValidatePasswordAsync(TenantUser user, string password)
        {
            return await Task.FromResult(BCrypt.Net.BCrypt.Verify(password, user.PasswordHash));
        }
    }

    public class TenantInfo
    {
        public string TenantId { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
    }

    public class TenantDto
    {
        public string TenantId { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string PrimaryRegion { get; set; } = string.Empty;
        public string CurrentVersion { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public bool IsHealthy { get; set; } = true;
        public Dictionary<string, string> CustomHeaders { get; set; } = new();
    }
}