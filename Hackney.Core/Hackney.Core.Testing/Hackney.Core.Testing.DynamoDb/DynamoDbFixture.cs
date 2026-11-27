using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hackney.Core.Testing.DynamoDb
{
    /// <summary>
    /// DynamoDb fixture class to be used to set up a local database instance for use in tests where a 
    /// "real" instance is required.
    /// </summary>
    public class DynamoDbFixture : IDynamoDbFixture
    {
        /// <summary>
        /// A IDynamoDBContext reference
        /// </summary>
        public IDynamoDBContext DynamoDbContext { get; private set; }

        /// <summary>
        /// A IAmazonDynamoDB reference
        /// </summary>
        public IAmazonDynamoDB DynamoDb { get; private set; }

        private readonly List<Func<Task>> _cleanup = new List<Func<Task>>();
        private bool _disposed;

        public DynamoDbFixture(IDynamoDBContext dynamoDbContext, IAmazonDynamoDB dynamoDb)
        {
            DynamoDb = dynamoDb;
            DynamoDbContext = dynamoDbContext;
        }

        /// <summary>
        /// Deletes entities registered by <see cref="SaveEntityAsync{T}"/>.
        /// Prefer <see cref="DisposeAsync"/> when the caller can await; this sync path waits for the same cleanup.
        /// </summary>
        public void Dispose()
        {
            DisposeAsync().AsTask().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Deletes entities registered by <see cref="SaveEntityAsync{T}"/> and awaits those deletes.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            foreach (var cleanup in _cleanup)
                await cleanup().ConfigureAwait(false);

            _cleanup.Clear();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Uses the configured DynamoDb instance to created the required table(s). 
        /// If the table already exists then the ResourceInUseException is *not* rethrown.
        /// </summary>
        /// <param name="tables">List of table definitions to create</param>
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

        /// <summary>
        /// Saves the supplied entity to the configured DynamoDb instance. 
        /// Also retains a reference to the saved entity and will remove it from the database automatically when the fixture is disposed.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="entity">The entity instance</param>
        /// <returns>Task</returns>
        public async Task SaveEntityAsync<T>(T entity) where T : class
        {
            await DynamoDbContext.SaveAsync<T>(entity).ConfigureAwait(false);
            _cleanup.Add(async () =>
            {
                try
                {
                    await DynamoDbContext.DeleteAsync(entity).ConfigureAwait(false);
                }
                catch (ConditionalCheckFailedException ex)
                {
                    // Ignore this exception as it means the entity has already been deleted
                    Console.WriteLine($"Delete failed: {ex.Message}");
                }
            });
        }
    }
}
