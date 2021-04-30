using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Hackney.Core.Tests.Middleware
{
    internal class DummyResponseFeature : IHttpResponseFeature
    {
        public Stream Body { get; set; }
        public bool HasStarted { get { return hasStarted; } }
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public string ReasonPhrase { get; set; }
        public int StatusCode { get; set; }

        public void OnCompleted(Func<object, Task> callback, object state)
        {
            //...No-op
        }

        public void OnStarting(Func<object, Task> callback, object state)
        {
            this.callback = callback;
            this.state = state;
        }

        bool hasStarted = false;
        Func<object, Task> callback;
        object state;

        public Task InvokeCallBack()
        {
            hasStarted = true;
            return callback(state);
        }
    }
}
