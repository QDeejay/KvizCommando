// src/Shared/Contracts/CheckIn/CheckInPostRequest.cs

    namespace KvizCommando.Shared.Contracts.CheckIn
    {
        /// <summary>
        /// POST /api/check-in kérés: opcionális megjelenített név + kötelező Terms verzió.
        /// </summary>
        public sealed class CheckInPostRequest
        {
            /// <summary>
            /// Opcionális. Ha a felhasználónak nincs DisplayName-je, itt küldi meg.
            /// </summary>
            public string? DisplayName { get; set; }
             public string? TeamName { get; set; }
          

        /// <summary>
        /// Kötelező. A felhasználó által elfogadott Terms verziója (vagy hash),
        /// igazodva a <see cref="TermsMeta"/> reprezentációjához.
        /// </summary>
        public string AcceptedTermsVersion { get; set; } = string.Empty;
        }
    }

