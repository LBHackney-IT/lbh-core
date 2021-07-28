using Microsoft.AspNetCore.Http;

namespace Hackney.Core.JWT
{
    public interface ITokenFactory
    {
        public const string DefaultHeaderName = "Authorization";

        Token Create(IHeaderDictionary headerDictionary, string headerName = DefaultHeaderName);
    }
}
