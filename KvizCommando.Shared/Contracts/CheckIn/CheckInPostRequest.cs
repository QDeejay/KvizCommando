

    namespace KvizCommando.Shared.Contracts.CheckIn
    {
        /// <summary>
        /// A beléptetés befejezéséhez megadott játékosnév és ÁSZF-verzió.
        /// </summary>
        public sealed class CheckInPostRequest
        {
            /// <summary>
            /// A felhasználó nyilvános játékosneve. Már beállított név esetén elhagyható.
            /// </summary>
            public string? DisplayName { get; set; }
            public string? TeamName { get; set; }
            public string? SessionId { get; set; }
          

        /// <summary>
        /// A felhasználó által elfogadott ÁSZF <see cref="TermsMeta.Version"/> értéke.
        /// </summary>
        public string AcceptedTermsVersion { get; set; } = string.Empty;
        }
    }
