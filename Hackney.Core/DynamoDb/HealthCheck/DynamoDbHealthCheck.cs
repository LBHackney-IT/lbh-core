using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hackney.Core.DynamoDb.HealthCheck
{
    public class DynamoDbHealthCheck<T> : IHealthCheck where T : class
    {
        private readonly IAmazonDynamoDB _client;
        private readonly string _tableName;

        public DynamoDbHealthCheck(IAmazonDynamoDB client)
        {
            _client = client;
            _tableName = GetTableName();
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await _client.DescribeTableAsync(_tableName, cancellationToken).ConfigureAwait(false);
                return HealthCheckResult.Healthy($"Can successfully access the {_tableName} table details in the DynamoDb instance");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Cannot access {_tableName} in the DynamoDb instance", ex);
            }
        }

        private static string GetTableName()
        {
            var type = typeof(T);
            var tableAttribute = type.GetCustomAttributes(true)
                                     .FirstOrDefault(x => x is DynamoDBTableAttribute) as DynamoDBTableAttribute;

            if (tableAttribute is null) throw new ArgumentException($"Type {type.Name} does not have the DynamoDBTable attribute applied to it.");

            return tableAttribute.TableName;
        }
    }
}
