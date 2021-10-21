using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hackney.Core.Testing.DynamoDb
{
    public interface IDynamoDbFixture
    {
        IDynamoDBContext DynamoDbContext { get; }
        IAmazonDynamoDB DynamoDb { get; }

        void EnsureTablesExist(List<TableDef> tables);
        Task SaveEntityAsync<T>(T entity) where T : class;
    }
}
