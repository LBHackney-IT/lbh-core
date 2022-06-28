using System;
using System.Collections.Generic;
using System.Net;

namespace Hackney.Core.Http.Exceptions
{
    /// <summary>
    /// Exception class used by the <see cref="ApiGateway"/> implementation
    /// </summary>
    public class ApiException : Exception
    {
        /// <summary>
        /// The entity type requested
        /// </summary>
        public string EntityType { get; }

        /// <summary>
        /// The route used by the request
        /// </summary>
        public string Route { get; }

        /// <summary>
        /// The headers used in the request
        /// </summary>
        public IEnumerable<KeyValuePair<string, IEnumerable<string>>> Headers { get; }

        /// <summary>
        /// The response status code
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// The response body
        /// </summary>
        public string ResponseBody { get; }

        public ApiException(string type, string route, HttpStatusCode statusCode, string responseBody)
            : this(type, route, new List<KeyValuePair<string, IEnumerable<string>>>(), statusCode, responseBody)
        { }

        public ApiException(string type, string route, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers, HttpStatusCode statusCode, string responseBody)
            : base($"Request failed. Route: {route}; Status code: {statusCode}; Message: {responseBody}")
        {
            EntityType = type;
            Route = route;
            Headers = headers;
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}
