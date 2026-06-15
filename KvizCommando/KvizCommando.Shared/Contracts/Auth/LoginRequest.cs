using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KvizCommando.Shared.Contracts.Auth
{
    /// <summary>
    /// WASM kliens bejelentkezési kérés szerződés (DTO).
    /// </summary>
    public  class LoginRequestDto
    {
        [Required]
        public string Email { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;

        public string twoFactorCode { get; set; } = string.Empty!;
        public string twoFactorRecoveryCode { get; set; } = string.Empty!;

        
    }
    public sealed class LoginRequestForm : LoginRequestDto
    {
        public bool rememberMe { get; set; } = false;
    }
}
