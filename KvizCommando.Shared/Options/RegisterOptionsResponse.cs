using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KvizCommando.Shared.Options
{
   
        public class RegisterOptionsResponse
        {
            // Jelszóházirend
            public int RequiredLength { get; set; }
            public bool RequireNonAlphanumeric { get; set; }
            public bool RequireDigit { get; set; }
            public bool RequireLowercase { get; set; }
            public bool RequireUppercase { get; set; }
            public int RequiredUniqueChars { get; set; }

            // Felhasználói fiók
            public bool RequireUniqueEmail { get; set; }

            // Bejelentkezési követelmények
            public bool RequireConfirmedEmail { get; set; }
            public bool RequireConfirmedAccount { get; set; }

            // Alkalmazásspecifikus szabályok
            public int DisplayNameMaxLength { get; set; }
            public int DisplayNameMinLength { get; set; }

        }
 }
