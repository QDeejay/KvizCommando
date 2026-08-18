using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KvizCommando.Shared.Contracts.Auth
{
    public sealed class ForgotPasswordRequestForm
    {
        public string email { get; set; } = string.Empty;
    }
}
