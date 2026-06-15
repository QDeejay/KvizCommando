using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KvizCommando.Shared.Options
{
   
        public class RegisterOptionsResponse
        {
            // Password policy
            public int RequiredLength { get; set; }
            public bool RequireNonAlphanumeric { get; set; }
            public bool RequireDigit { get; set; }
            public bool RequireLowercase { get; set; }
            public bool RequireUppercase { get; set; }
            public int RequiredUniqueChars { get; set; }

            // User policy
            public bool RequireUniqueEmail { get; set; }

            // SignIn policy
            public bool RequireConfirmedEmail { get; set; }
            public bool RequireConfirmedAccount { get; set; }

            // Custom rules
            public int DisplayNameMaxLength { get; set; }
            public int DisplayNameMinLength { get; set; }

            //public bool AcceptTermsRequired { get; set; }
            //public string PreferredLocaleRegex { get; set; } = "";
        }
 }

