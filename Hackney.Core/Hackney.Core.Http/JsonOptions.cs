using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hackney.Core.Http
{
    /// <summary>
    /// Static class used to create a standard JsonOptions object for use in object (de)serialisation.
    /// </summary>
    public static class JsonOptions
    {
        /// <summary>
        /// Creates a standard JsonOptions object for use in object (de)serialisation.
        /// * Camel case naming 
        /// * Indents each line
        /// * Uses the JsonStringEnumConverter for enum values
        /// </summary>
        /// <returns>JsonOptions</returns>
        public static JsonSerializerOptions Create()
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
