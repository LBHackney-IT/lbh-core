using AutoFixture;
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
        private readonly Fixture _fixture = new Fixture();
        private readonly JsonSerializerOptions _jsonOptions;
        private static HttpListener _httpListener;
        public static T ResponseObject { get; private set; }
        public List<string> ReceivedCorrelationIds { get; private set; } = new List<string>();
        public static string ApiRoute => "http://localhost:5678/api/v1/";
        public static string ApiToken => "sdjkhfgsdkjfgsdjfgh";
        public List<T> Responses { get; private set; } = new List<T>();

        public BaseApiFixture()
        {
            _jsonOptions = CreateJsonOptions();
            StartApiStub();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (_httpListener.IsListening)
                    _httpListener.Stop();
                ResponseObject = null;

                _disposed = true;
            }
        }

        private JsonSerializerOptions CreateJsonOptions()
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
            Environment.SetEnvironmentVariable("ApiUrl", ApiRoute);
            Environment.SetEnvironmentVariable("ApiToken", ApiToken);
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
                    HttpListenerResponse response = context.Response;

                    if (context.Request.Headers["Authorization"] != ApiToken)
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
                            // thisResponse = Responses.FirstOrDefault(x => x.Id.ToString() == requestedId);
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