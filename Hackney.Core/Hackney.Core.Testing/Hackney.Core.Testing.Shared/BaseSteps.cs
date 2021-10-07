using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hackney.Core.Testing.Shared
{
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