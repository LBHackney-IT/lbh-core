using Newtonsoft.Json;

namespace Hackney.Core.JWT
{
    /// <summary>
    /// Presentation layer object representing what Token could be.
    /// A seperate object is needed to avoid breaking the existing
    /// Domain layer contract. Do not use outside Hackney.Core.JWT
    /// </summary>
    public class TokenPresentation
    {
        public string Sub { get; set; }

        // The old token format groups field
        public string[]? Groups { get; set; }

        // New cognito token format field
        [JsonProperty("custom:groups")]
        public string? CustomGroups { get; set; }

        public string Email { get; set; }
        public string Name { get; set; }
        public long Nbf { get; set; }
        public long Exp { get; set; }
        public long Iat { get; set; }
    }
}
