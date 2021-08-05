using Microsoft.AspNetCore.Http;

namespace Hackney.Core.JWT
{
    /// <summary>
    /// Interface defining the creation of a token object from a JWT included in Http headers
    /// </summary>
    public interface ITokenFactory
    {
        /// <summary>
        /// The default Http header name used to create a token
        /// </summary>
        public const string DefaultHeaderName = "Authorization";

        /// <summary>
        /// Extracts a JWT from the supplied Http headers and creates a token object from it.
        /// </summary>
        /// <param name="headerDictionary">The Http headers</param>
        /// <param name="headerName">The header key name used. Default: "Authorization"</param>
        /// <returns>The deserialised Token or null</returns>
        /// <exception cref="System.ArgumentNullException">If the headerDictionary is null, or the header name is empty.</exception>
        Token Create(IHeaderDictionary headerDictionary, string headerName = DefaultHeaderName);
    }
}
