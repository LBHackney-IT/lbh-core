using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hackney.Core.DynamoDb.Converters
{
    public class DynamoDbEnumListConverter<TEnum> : IPropertyConverter where TEnum : Enum
    {
        public DynamoDBEntry ToEntry(object value)
        {
            if (null == value) return new DynamoDBNull();

            var list = value as IEnumerable<TEnum>;
            if (null == list)
                throw new ArgumentException($"Field value is not a list of {typeof(TEnum).Name}. This attribute has been used on a property that is not a list of enum values.");

            return new DynamoDBList(list.Select(x => new Primitive(Enum.GetName(typeof(TEnum), x))));
        }

        public object FromEntry(DynamoDBEntry entry)
        {
            if ((null == entry) || (null != entry.AsDynamoDBNull())) return null;

            var list = entry.AsDynamoDBList();
            if (null == list)
                throw new ArgumentException("Field value is not a DynamoDBList. This attribute has been used on a property that is not a list of enum values.");

            return list.AsListOfDynamoDBEntry().Select(x =>
                    (TEnum)Enum.Parse(typeof(TEnum), x.AsString())).ToList();
        }
    }
}
