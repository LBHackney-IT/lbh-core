using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hackney.Core.Testing.Shared.E2E
{
    /// <summary>
    /// Base clase for BDDfy E2E test steps.
    /// This class is used when serialising objects to handle Camel Casing, Indentation, and conversion of enums to JSON strings.
    /// It sets up the HttpClient, which is used to send HTTP requests and retrieve responses
    /// </summary>
    public class BaseSteps
    {
        protected readonly HttpClient _httpClient;

        protected HttpResponseMessage _lastResponse;
        protected readonly JsonSerializerOptions _jsonOptions;

        public BaseSteps(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = CreateJsonOptions();
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
    }
}