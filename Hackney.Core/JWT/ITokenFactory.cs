using Microsoft.AspNetCore.Http;

namespace Hackney.Core.JWT
{
    public interface ITokenFactory
    {
        Token Create(IHeaderDictionary headerDictionary);
    }
}
