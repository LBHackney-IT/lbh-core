using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hackney.Core.Testing.DynamoDb
{
    public class DynamoDbFixture : IDynamoDbFixture, IDisposable
    {
        public IDynamoDBContext DynamoDbContext { get; private set; }
        public IAmazonDynamoDB DynamoDb { get; private set; }

        private static List<Action> _cleanup = new List<Action>();

        public DynamoDbFixture(IDynamoDBContext dynamoDbContext, IAmazonDynamoDB dynamoDb)
        {
            DynamoDb = dynamoDb;
            DynamoDbContext = dynamoDbContext;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                foreach (var act in _cleanup)
                    act();

                _disposed = true;
            }
        }

        public void EnsureTablesExist(List<TableDef> tables)
        {
            foreach (var table in tables)
            {
                try
                {
                    var keySchema = new List<KeySchemaElement> { new KeySchemaElement(table.KeyName, KeyType.HASH) };
                    var attributes = new List<AttributeDefinition> { new AttributeDefinition(table.KeyName, table.KeyType) };
                    if (!string.IsNullOrEmpty(table.RangeKeyName))
                    {
                        keySchema.Add(new KeySchemaElement(table.RangeKeyName, KeyType.RANGE));
                        attributes.Add(new AttributeDefinition(table.RangeKeyName, table.RangeKeyType));
                    }

                    foreach (var localIndex in table.LocalSecondaryIndexes)
                    {
                        var indexRangeKey = localIndex.KeySchema.FirstOrDefault(y => y.KeyType == KeyType.RANGE);
                        if ((null != indexRangeKey) && (!attributes.Any(x => x.AttributeName == indexRangeKey.AttributeName)))
                            attributes.Add(new AttributeDefinition(indexRangeKey.AttributeName, ScalarAttributeType.S)); // Assume a string for now.
                    }

                    foreach (var globalIndex in table.GlobalSecondaryIndexes)
                    {
                        foreach (var key in globalIndex.KeySchema)
                        {
                            if (!attributes.Any(x => x.AttributeName == key.AttributeName))
                                attributes.Add(new AttributeDefinition(key.AttributeName, ScalarAttributeType.S)); // Assume a string for now.
                        }
                    }

                    var request = new CreateTableRequest(table.Name,
                        keySchema,
                        attributes,
                        new ProvisionedThroughput(3, 3))
                    {
                        LocalSecondaryIndexes = table.LocalSecondaryIndexes,
                        GlobalSecondaryIndexes = table.GlobalSecondaryIndexes
                    };
                    _ = DynamoDb.CreateTableAsync(request).GetAwaiter().GetResult();
                }
                catch (ResourceInUseException)
                {
                    // It already exists :-)
                }
            }
        }

        public async Task SaveEntityAsync<T>(T entity) where T : class
        {
            await DynamoDbContext.SaveAsync<T>(entity).ConfigureAwait(false);
            _cleanup.Add(async () => await DynamoDbContext.DeleteAsync(entity).ConfigureAwait(false));
        }
    }
}
