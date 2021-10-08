using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Hackney.Core.Validation.AspNet
{
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// In order to use this method in a controller the request body must be made available using the EnableRequestBodyRewind middleware.
        /// </summary>
        /// <param name="request">An HttpRequest</param>
        /// <param name="encoding">The text encoding. (Default: UTF8)</param>
        /// <returns>The request's body text</returns>
        public static async Task<string> GetRawBodyStringAsync(this HttpRequest request, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            request.Body.Seek(0, SeekOrigin.Begin);
            using (StreamReader reader = new StreamReader(request.Body, encoding))
                return await reader.ReadToEndAsync().ConfigureAwait(false);
        }
    }
}