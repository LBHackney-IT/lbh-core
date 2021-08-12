using Microsoft.AspNetCore.Http;

namespace Hackney.Core.Http
{
    /// <summary>
    /// Helper for accessing information on an HttpContext
    /// </summary>
    public interface IHttpContextWrapper
    {
        /// <summary>
        /// Retrieve the request headers from the supplied HttpContext
        /// </summary>
        /// <param name="context">The HttpContext</param>
        /// <returns>the request header dictionary or null</returns>
        IHeaderDictionary GetContextRequestHeaders(HttpContext context);
    }
}
