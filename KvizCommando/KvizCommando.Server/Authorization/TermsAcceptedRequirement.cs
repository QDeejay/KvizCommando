using Microsoft.AspNetCore.Authorization;

namespace KvizCommando.Server.Authorization
{
    /// <summary>
    /// Marker requirement az aktuális ÁSZF (Terms) elfogadásának érvényesítéséhez.
    /// A policy neve: <see cref="PolicyName"/>.
    /// </summary>
    public sealed class TermsAcceptedRequirement : IAuthorizationRequirement
    {
        /// <summary>Az ehhez a requirementhez tartozó policy neve.</summary>
        public const string PolicyName = "RequireCurrentTerms";
    }
}
