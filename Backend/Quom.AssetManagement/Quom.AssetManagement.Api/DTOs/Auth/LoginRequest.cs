using System.ComponentModel.DataAnnotations;

namespace Quom.AssetManagement.Api.DTOs.Auth
{
    public class LoginRequest
    {
        [Required]
        [MaxLength(150)]
        public string Login { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Password { get; set; } = string.Empty;
    }
}