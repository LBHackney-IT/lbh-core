using Hackney.Core.Http.Exceptions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hackney.Core.Http
{
    /// <summary>
    /// Class defining a generic ApiGateway used to retrieve entities by id
    /// </summary>
    public class ApiGateway : IApiGateway
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly static JsonSerializerOptions _jsonOptions = JsonOptions.Create();

        /// <summary>
        /// The base uri route for the Api
        /// </summary>
        public string ApiRoute { get; private set; }

        /// <summary>
        /// A token to be used in each call to the Api
        /// </summary>
        public string ApiToken { get; private set; }

        /// <summary>
        /// The Api name
        /// </summary>
        public string ApiName { get; private set; }

        /// <summary>
        /// Any headers to be added to each call to the Api
        /// </summary>
        public Dictionary<string, string> RequestHeaders { get; private set; }

        private bool _useApiKey;
        private bool _initialised = false;

        public ApiGateway(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        /// <summary>
        /// Initiliases the Gateway to use the specified Api details.
        /// </summary>
        /// <param name="apiName">The Api name</param>
        /// <param name="configKeyApiUrl">The configuration key containing the base uri route for the Api</param>
        /// <param name="configKeyApiToken">The configuration key containing the token to be used with the Api</param>
        /// <param name="headers">Any heasders to be used when calling the Api (optional)</param>
        public void Initialise(string apiName, string configKeyApiUrl, string configKeyApiToken, Dictionary<string, string> headers = null, bool useApiKey = false)
        {
            if (string.IsNullOrEmpty(apiName)) throw new ArgumentNullException(nameof(apiName));
            ApiName = apiName;

            var apiRoute = _configuration.GetValue<string>(configKeyApiUrl)?.TrimEnd('/');
            if (string.IsNullOrEmpty(apiRoute) || !Uri.IsWellFormedUriString(apiRoute, UriKind.Absolute))
                throw new ArgumentException($"Configuration does not contain a setting value for the key {configKeyApiUrl}.");
            ApiRoute = apiRoute;

            var apiToken = _configuration.GetValue<string>(configKeyApiToken);
            if (string.IsNullOrEmpty(apiToken))
                throw new ArgumentException($"Configuration does not contain a setting value for the key {configKeyApiToken}.");
            ApiToken = apiToken;

            RequestHeaders = headers ?? new Dictionary<string, string>();

            _useApiKey = useApiKey;
            _initialised = true;
        }

        /// <summary>
        /// Makes a basic GET call to the Api to retrieve the requestsed entity details from it.
        /// Includes the token and headers set on the class.
        /// </summary>
        /// <typeparam name="T">The entity type required</typeparam>
        /// <param name="route">The full route to the GET endpoint</param>
        /// <param name="id">The id of the requested object</param>
        /// <param name="correlationId">The correlation id to use on the request.</param>
        /// <returns>The requested entity if found. null if not found</returns>
        /// <exception cref="GetFromApiException">If the Http GET request returns anything other than a success status code or not found</exception>
        public async Task<T> GetByIdAsync<T>(string route, Guid id, Guid correlationId) where T : class
        {
            if (!_initialised) throw new InvalidOperationException("Initialise() must be called before any other calls are made");

            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-correlation-id", correlationId.ToString());

            if (_useApiKey)
                client.DefaultRequestHeaders.Add("x-api-key", AuthenticationHeaderValue.Parse(ApiToken).ToString());
            else
                client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(ApiToken);

            foreach (var pair in RequestHeaders)
                client.DefaultRequestHeaders.Add(pair.Key, pair.Value);

            var response = await client.GetAsync(new Uri(route))
                                       .ConfigureAwait(false);

            if (response.StatusCode is HttpStatusCode.NotFound)
                return null;

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
                return JsonSerializer.Deserialize<T>(responseBody, _jsonOptions);

            throw new GetFromApiException(ApiName, route, client.DefaultRequestHeaders.ToList(),
                                          id, response.StatusCode, responseBody);
        }
    }
}
