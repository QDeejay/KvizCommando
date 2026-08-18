using Microsoft.AspNetCore.Authorization;

namespace KvizCommando.Server.Authorization
{
    /// <summary>
    /// Marker requirement az aktuális ÁSZF (Terms) elfogadásának érvényesítéséhez.
    /// A policy neve: <see cref="POLICY_NAME"/>.
    /// </summary>
    public sealed class TermsAcceptedRequirement : IAuthorizationRequirement
    {
        /// <summary>Az ÁSZF-elfogadási követelményhez tartozó jogosultsági szabály neve.</summary>
        public const string POLICY_NAME = "RequireCurrentTerms";
    }
}
