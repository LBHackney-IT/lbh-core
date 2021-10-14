using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hackney.Core.Testing.Shared.E2E
{
    /// <summary>
    /// Base class for BDDfy E2E test fixtures that access an external API.
    /// Creates an HttpListener to act as a stub Api.
    /// </summary>
    /// <typeparam name="T">Type of the response object this fixture provides</typeparam>
    public class BaseApiFixture<T> : IDisposable where T : class
    {
        protected readonly JsonSerializerOptions _jsonOptions;
        private HttpListener _httpListener;

        /// <summary>
        /// List of all requests made to the stub Api.
        /// </summary>
        public List<HttpListenerRequest> Requests { get; private set; } = new List<HttpListenerRequest>();

        /// <summary>
        /// A single response object to be returned when the stub Api is called.
        /// </summary>
        public T ResponseObject { get; protected set; }

        /// <summary>
        /// List of correlation ids present all requests made to the stub Api.
        /// </summary>
        public List<string> ReceivedCorrelationIds { get; protected set; } = new List<string>();

        /// <summary>
        /// The base route for the stub Api
        /// </summary>
        public string ApiRoute { get; protected set; }

        /// <summary>
        /// An authorisation token that will be expected in any calls to the stub Api.
        /// </summary>
        public string ApiToken { get; protected set; }

        /// <summary>
        /// A list ot response objects, keyed by id, to be returned when the stub Api is called. 
        /// </summary>
        public Dictionary<string, T> Responses { get; protected set; } = new Dictionary<string, T>();

        /// <summary>
        /// The number of calls made to the stub Api
        /// </summary>
        public int CallsMade { get; private set; }

        public BaseApiFixture(string route)
        {
            ApiRoute = route ?? throw new ArgumentNullException(nameof(route));

            _jsonOptions = CreateJsonOptions();
            StartApiStub();
        }
        public BaseApiFixture(string route, string token)
        {
            ApiRoute = route ?? throw new ArgumentNullException(nameof(route));
            ApiToken = token ?? throw new ArgumentNullException(nameof(token));

            _jsonOptions = CreateJsonOptions();
            StartApiStub();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (_httpListener.IsListening)
                    _httpListener.Stop();

                ResponseObject = null;
                Responses.Clear();

                _disposed = true;
            }
        }

        protected JsonSerializerOptions CreateJsonOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        private void StartApiStub()
        {
            CallsMade = 0;
            Requests.Clear();
            ReceivedCorrelationIds.Clear();

            Task.Run(() =>
            {
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add(ApiRoute);
                _httpListener.Start();

                // GetContext method blocks while waiting for a request.
                while (true)
                {
                    HttpListenerContext context = _httpListener.GetContext();
                    CallsMade++;
                    Requests.Add(context.Request);
                    HttpListenerResponse response = context.Response;

                    if (!string.IsNullOrEmpty(ApiToken)
                        && (context.Request.Headers["Authorization"] != ApiToken))
                    {
                        response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    }
                    else
                    {
                        ReceivedCorrelationIds.Add(context.Request.Headers["x-correlation-id"]);
                        var thisResponse = ResponseObject;
                        if (Responses.Any())
                        {
                            var requestedId = context.Request.Url.Segments.Last();
                            thisResponse = Responses.ContainsKey(requestedId) ? Responses[requestedId] : null;
                        }

                        response.StatusCode = (int)((thisResponse is null) ? HttpStatusCode.NotFound : HttpStatusCode.OK);
                        string responseBody = string.Empty;
                        if (thisResponse is null)
                        {
                            responseBody = context.Request.Url.Segments.Last();
                        }
                        else
                        {
                            responseBody = JsonSerializer.Serialize(thisResponse, _jsonOptions);
                        }
                        Stream stream = response.OutputStream;
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.Write(responseBody);
                            writer.Close();
                        }
                    }
                }
            });
        }
    }
}