using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KvizCommando.Shared.Contracts.Auth
{
    /// <summary>
    /// A kliens regisztrációs kérésének közös adatszerződése.
    /// A jelszóházirendet véglegesen a szerver Identity-beállításai érvényesítik.
    /// </summary>
  

    public class RegisterRequestDto 
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        

        
    }
    public sealed class RegisterRequestForm : RegisterRequestDto
    {
        public string ConfirmPassword { get; set; } = default!;
    }
}

