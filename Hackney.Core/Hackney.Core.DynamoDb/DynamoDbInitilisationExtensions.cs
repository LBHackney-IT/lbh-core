using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hackney.Core.DynamoDb
{
    public static class DynamoDbInitilisationExtensions
    {
        /// <summary>
        /// Configures an application to use DynamoDb
        /// If the environment variable DynamoDb_LocalMode is set to true then it will use the 
        /// DynamoDb_LocalServiceUrl environment variable to determine the instance address.
        /// If the environment variable DynamoDb_LocalMode is set to false then it  assumes that the 
        /// application's exectuion context will have the necessary connection information available.
        /// </summary>
        /// <param name="services">A service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection ConfigureDynamoDB(this IServiceCollection services)
        {
            bool localMode = false;
            _ = bool.TryParse(Environment.GetEnvironmentVariable("DynamoDb_LocalMode"), out localMode);

            if (localMode)
            {
                var url = Environment.GetEnvironmentVariable("DynamoDb_LocalServiceUrl");
                services.AddSingleton<IAmazonDynamoDB>(sp =>
                {
                    var clientConfig = new AmazonDynamoDBConfig { ServiceURL = url };
                    return new AmazonDynamoDBClient(clientConfig);
                });
            }
            else
            {
                services.AddAWSService<IAmazonDynamoDB>();
            }

            services.AddScoped<IDynamoDBContext>(sp =>
            {
                var db = sp.GetService<IAmazonDynamoDB>();
                return new DynamoDBContext(db);
            });

            return services;
        }
    }
}
