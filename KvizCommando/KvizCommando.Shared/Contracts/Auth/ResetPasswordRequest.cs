using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KvizCommando.Shared.Contracts.Auth
{
    /// <summary>
    /// Jelszó visszaállítására szolgáló kérés szerződés.
    /// </summary>
    public  class ResetPasswordDto
    {
        [Required]
        [EmailAddress]
        public string email { get; set; } = default!;

        [Required]
        public string resetCode { get; set; } = default!;

        [Required]
    
        public string newPassword { get; set; } = default!;

       
    }
    public sealed class ResetPasswordForm : ResetPasswordDto
    {
        public string confirmedNewPsw { get; set; } = default!;

    }

}
