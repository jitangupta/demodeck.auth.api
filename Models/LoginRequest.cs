using System.ComponentModel.DataAnnotations;

namespace DemoDeck.Auth.Api.Models
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required] 
        public string Password { get; set; } = string.Empty;
        
        [Required]
        public string TenantName { get; set; } = string.Empty;
    }
}