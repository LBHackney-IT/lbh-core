using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hackney.Core.Http
{
    /// <summary>
    /// Interface defining a generic ApiGateway used to retrieve entities by id
    /// </summary>
    public interface IApiGateway
    {
        /// <summary>
        /// The base uri route for the Api
        /// </summary>
        string ApiRoute { get; }

        /// <summary>
        /// A token to be used in each call to the Api
        /// </summary>
        string ApiToken { get; }

        /// <summary>
        /// The Api name
        /// </summary>
        string ApiName { get; }

        /// <summary>
        /// Any headers to be added to each call to the Api
        /// </summary>
        Dictionary<string, string> RequestHeaders { get; }

        /// <summary>
        /// Initiliases the Gateway to use the specified Api details.
        /// </summary>
        /// <param name="apiName">The Api name</param>
        /// <param name="configKeyApiUrl">The configuration key containing the base uri route for the Api</param>
        /// <param name="configKeyApiToken">The configuration key containing the token to be used with the Api</param>
        /// <param name="headers">Any heasders to be used when calling the Api (optional)</param>
        /// <param name="useApiKey">Set to 'true' if this API needs an 'x-api-key' header rather than 'Authorization' (optional)</param>
        void Initialise(string apiName, string configKeyApiUrl, string configKeyApiToken, Dictionary<string, string> headers = null, bool useApiKey = false);

        /// <summary>
        /// Makes a basic GET call to the Api to retrieve the requestsed entity details from it
        /// </summary>
        /// <typeparam name="T">The entity type required</typeparam>
        /// <param name="route">The full route to the GET endpoint</param>
        /// <param name="id">The id of the requested object</param>
        /// <param name="correlationId">The correlation id to use on the request.</param>
        /// <returns>The requested entity</returns>
        Task<T> GetByIdAsync<T>(string route, Guid id, Guid correlationId) where T : class;
    }
}
