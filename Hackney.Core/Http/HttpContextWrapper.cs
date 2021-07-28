using Microsoft.AspNetCore.Http;

namespace Hackney.Core.Http
{
    public class HttpContextWrapper : IHttpContextWrapper
    {
        public IHeaderDictionary GetContextRequestHeaders(HttpContext context)
        { 
            return context?.Request?.Headers;
        }
    }
}