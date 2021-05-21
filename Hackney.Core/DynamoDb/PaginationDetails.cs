using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

namespace Hackney.Core.DynamoDb
{
    /// <summary>
    /// Class encapsulting details used for pagination
    /// </summary>
    public class PaginationDetails
    {
        [JsonIgnore]
        public bool HasNext => !string.IsNullOrEmpty(NextToken);
        public string NextToken { get; set; }

        public PaginationDetails() { }
        public PaginationDetails(string rawNextToken)
        {
            EncodeNextToken(rawNextToken);
        }

        /// <summary>
        /// Encodes the pagination token returned by DynamoDb as a Base64 string.
        /// </summary>
        /// <param name="rawToken">The raw pagination token returned by DynamoDb</param>
        /// <returns>The token as a Base64 string</returns>
        public static string EncodeToken(string rawToken)
        {
            // The AWS SDK can return an empty JSON object (i.e. '{}') when there are no more results.
            if (string.IsNullOrWhiteSpace(rawToken?.Trim(' ', '{', '}')))
                return null;

            return Base64UrlEncoder.Encode(rawToken);
        }

        /// <summary>
        /// Decodes the Base64 pagination token.
        /// </summary>
        /// <param name="rawToken">The Base64 token value</param>
        /// <returns>The decoded token</returns>
        public static string DecodeToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            return Base64UrlEncoder.Decode(token);
        }

        /// <summary>
        /// Encodes and sets the DynamoDb pagination token in the NextToken property 
        /// </summary>
        /// <param name="rawToken">The raw pagination token returned by DynamoDb</param>
        public void EncodeNextToken(string rawToken)
        {
            NextToken = EncodeToken(rawToken);
        }

        /// <summary>
        /// Decodes the NextToken property value into the raw DynamoDb token
        /// </summary>
        /// <returns>The raw DynamoDb token</returns>
        public string DecodeNextToken()
        {
            return DecodeToken(NextToken);
        }
    }
}
