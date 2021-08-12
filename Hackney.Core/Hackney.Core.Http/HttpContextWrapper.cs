using Microsoft.AspNetCore.Http;

namespace Hackney.Core.Http
{
    /// <summary>
    /// Helper for accessing information on an HttpContext
    /// </summary>
    public class HttpContextWrapper : IHttpContextWrapper
    {
        /// <summary>
        /// Retrieve the request headers from the supplied HttpContext
        /// </summary>
        /// <param name="context">The HttpContext</param>
        /// <returns>the request header dictionary or null</returns>
        public IHeaderDictionary GetContextRequestHeaders(HttpContext context)
        {
            return context?.Request?.Headers;
        }
    }
}