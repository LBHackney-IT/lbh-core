using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace Hackney.Core.DynamoDb.Converters
{
    /// <summary>
    /// Converter to be used bool properties.
    /// Ensures value is converted as a bool (not an int) and means the property can be used in filters.
    /// </summary>
    public class DynamoDbBoolConverter : IPropertyConverter
    {
        public DynamoDBEntry ToEntry(object value)
        {
            if (null == value) return new DynamoDBNull();

            return new DynamoDBBool((bool) value);
        }

        public object FromEntry(DynamoDBEntry entry)
        {
            if (entry is null) return null;
            if (entry is DynamoDBBool) return entry.AsBoolean();
            return bool.Parse(entry.AsString());
        }
    }
}
