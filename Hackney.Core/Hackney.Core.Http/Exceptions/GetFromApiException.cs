using System;
using System.Collections.Generic;
using System.Net;

namespace Hackney.Core.Http.Exceptions
{
    /// <summary>
    /// Exception class used by the <see cref="ApiGateway"/> implementation
    /// </summary>
    public class GetFromApiException : Exception
    {
        /// <summary>
        /// The entity type requested
        /// </summary>
        public string EntityType { get; }

        /// <summary>
        /// The route used by the GET request
        /// </summary>
        public string Route { get; }

        /// <summary>
        /// The entity Id requested
        /// </summary>
        public Guid EntityId { get; }

        /// <summary>
        /// The headers used in the GET request
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

        public GetFromApiException(string type, string route, Guid id, HttpStatusCode statusCode, string responseBody)
            : this(type, route, new List<KeyValuePair<string, IEnumerable<string>>>(), id, statusCode, responseBody)
        { }

        public GetFromApiException(string type, string route, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers,
            Guid id, HttpStatusCode statusCode, string responseBody)
            : base($"Failed to get {type} details for id {id}. Route: {route}; Status code: {statusCode}; Message: {responseBody}")
        {
            EntityType = type;
            Route = route;
            Headers = headers;
            EntityId = id;
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}
