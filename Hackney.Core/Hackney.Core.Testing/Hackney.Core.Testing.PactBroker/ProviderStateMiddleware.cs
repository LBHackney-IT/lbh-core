using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hackney.Core.Testing.PactBroker
{
    /// <summary>
    /// Middleware class used to handle calls made to the API endpoints by the pact broker.
    /// It filters out calls made to the /provider-states route and passes them on to
    /// the registered <see cref="IPactBrokerHandler"/> instance to perform any necessary actions.
    /// </summary>
    public class ProviderStateMiddleware
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly IPactBrokerHandler _pactBrokerHandler;
        private readonly RequestDelegate _next;

        public ProviderStateMiddleware(RequestDelegate next, IServiceProvider services)
        {
            _next = next;
            _pactBrokerHandler = services.GetRequiredService<IPactBrokerHandler>();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!(context.Request.Path.Value?.StartsWith(Constants.PROVIDER_STATES_ROUTE) ?? false))
            {
                await _next.Invoke(context).ConfigureAwait(false);
                return;
            }

            context.Response.StatusCode = (int) HttpStatusCode.OK;

            if (context.Request.Method == HttpMethod.Post.ToString())
            {
                string jsonRequestBody;
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
                {
                    jsonRequestBody = await reader.ReadToEndAsync().ConfigureAwait(false);
                }

                var providerState = JsonSerializer.Deserialize<ProviderState>(jsonRequestBody, _options);

                //A null or empty provider state key must be handled
                if (!string.IsNullOrEmpty(providerState?.State))
                {
                    if (_pactBrokerHandler.ProviderStates.ContainsKey(providerState.State))
                    {
                        _pactBrokerHandler.ProviderStates[providerState.State].Invoke(providerState.State, providerState.Params);
                        await context.Response.WriteAsync($"Executed actions for provider state step: {providerState.State}")
                                     .ConfigureAwait(false);
                    }
                    else
                        await context.Response.WriteAsync($"No actions configured for provider state step: {providerState.State}")
                                              .ConfigureAwait(false);
                }
                else
                {
                    Console.WriteLine($"Provider state request is not valid. It is either not a ProviderState object or no State property is defined. Request body: ");
                    Console.WriteLine(jsonRequestBody);
                }
            }
        }
    }
}
