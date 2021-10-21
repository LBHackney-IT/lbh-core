using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Collections.Generic;

namespace Hackney.Core.Testing.DynamoDb
{
    public class TableDef
    {
        public string Name { get; set; }
        public string KeyName { get; set; }
        public ScalarAttributeType KeyType { get; set; }
        public string RangeKeyName { get; set; }
        public ScalarAttributeType RangeKeyType { get; set; }
        public List<LocalSecondaryIndex> LocalSecondaryIndexes { get; set; } = new List<LocalSecondaryIndex>();
        public List<GlobalSecondaryIndex> GlobalSecondaryIndexes { get; set; } = new List<GlobalSecondaryIndex>();
    }
}
