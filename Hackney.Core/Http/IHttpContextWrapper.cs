using Microsoft.AspNetCore.Http;

namespace Hackney.Core.Http
{
    public interface IHttpContextWrapper
    {
        IHeaderDictionary GetContextRequestHeaders(HttpContext context);
    }
}
