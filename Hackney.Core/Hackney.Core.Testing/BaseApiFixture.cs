using AutoFixture;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hackney.Core.Testing
{
    public abstract class BaseApiFixture<T> : IDisposable where T : class
    {
        protected readonly Fixture _fixture = new Fixture();
        protected readonly JsonSerializerOptions _jsonOptions;
        protected HttpListener _httpListener;

        protected string _route;
        protected string _token;

        public T ResponseObject { get; protected set; }
        public string ReceivedCorrelationId { get; private set; }

        protected BaseApiFixture()
        {
            _jsonOptions = JsonOptions.CreateJsonOptions();
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

                _disposed = true;
            }
        }

        protected virtual void StartApiStub()
        {
            ReceivedCorrelationId = null;
            Task.Run(() =>
            {
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add(_route);
                _httpListener.Start();

                // GetContext method blocks while waiting for a request. 
                HttpListenerContext context = _httpListener.GetContext();
                HttpListenerResponse response = context.Response;

                if (context.Request.Headers["Authorization"] != _token)
                {
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                }
                else
                {
                    ReceivedCorrelationId = context.Request.Headers["x-correlation-id"];

                    response.StatusCode = (int)((ResponseObject is null) ? HttpStatusCode.NotFound : HttpStatusCode.OK);
                    string responseBody = string.Empty;
                    if (ResponseObject is null)
                    {
                        responseBody = context.Request.Url.Segments.Last();
                    }
                    else
                    {
                        responseBody = JsonSerializer.Serialize(ResponseObject, _jsonOptions);
                    }
                    Stream stream = response.OutputStream;
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(responseBody);
                        writer.Close();
                    }
                }
            });
        }
    }
}