using Amazon.DynamoDBv2.DataModel;

namespace Hackney.Core.DynamoDb.Tests.HealthCheck
{
    public class TestModel
    {
        public string Id { get; set; }
    }

    [DynamoDBTable("Models")]
    public class TestModelDb
    {
        public string Id { get; set; }
    }
}
