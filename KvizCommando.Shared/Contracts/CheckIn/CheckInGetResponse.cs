namespace KvizCommando.Shared.Contracts.CheckIn
{
    public sealed class CheckInGetResponse
    {
        public bool Success { get; set; } = false; // user jogosult a check-in-re 
        /// <summary>
        /// Jelzi, hogy a felhasználónak meg kell-e adnia nyilvános játékosnevet.
        /// </summary>
        public bool NeedsDisplayName { get; set; }

        /// <summary>
        /// Jelzi, hogy a felhasználónak el kell-e fogadnia az aktuális ÁSZF-verziót.
        /// </summary>
        public bool NeedsTermsAcceptance { get; set; }

        /// <summary>
        /// Az aktuális ÁSZF verzió- és ellenőrzőadatai.
        /// </summary>
        public TermsMeta CurrentTerms { get; init; } = default!;

        public bool PreviousSessionReplaced { get; init; }

    }
}
