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
    public class BaseApiFixture<T> : IDisposable where T : class
    {
        /// <summary>
        // This fixture mocks an external API endpoint.
        /// </summary>

        protected readonly JsonSerializerOptions _jsonOptions;
        private HttpListener _httpListener;
        public T ResponseObject { get; protected set; }
        public List<string> ReceivedCorrelationIds { get; protected set; } = new List<string>();
        public string ApiRoute { get; protected set; }
        public string ApiToken { get; protected set; }
        public Dictionary<string, T> Responses { get; protected set; } = new Dictionary<string, T>();
        public int CallsMade { get; private set; }

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
                    HttpListenerResponse response = context.Response;

                    if (context.Request.Headers["Authorization"] != ApiToken)
                    {
                        response.StatusCode = (int) HttpStatusCode.Unauthorized;
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

                        response.StatusCode = (int) ((thisResponse is null) ? HttpStatusCode.NotFound : HttpStatusCode.OK);
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