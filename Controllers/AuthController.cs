using Microsoft.AspNetCore.Mvc;
using DemoDeck.Auth.Api.Models;
using DemoDeck.Auth.Api.Services;

namespace DemoDeck.Auth.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("token")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(LoginResponse), 400)]
        public async Task<IActionResult> GenerateToken([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new LoginResponse 
                { 
                    Success = false, 
                    Message = "Invalid input data" 
                });
            }

            var result = await _authService.AuthenticateAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("Successful authentication for user {Username} in tenant {TenantName}", 
                    request.Username, request.TenantName);
                return Ok(result);
            }

            _logger.LogWarning("Failed authentication attempt for user {Username} in tenant {TenantName}", 
                request.Username, request.TenantName);
            return BadRequest(result);
        }
    }
}