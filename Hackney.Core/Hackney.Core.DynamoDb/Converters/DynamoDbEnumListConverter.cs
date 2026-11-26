using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hackney.Core.DynamoDb.Converters;

/// <summary>
/// Converter for a list of enums where the value stored should be the enum value name (not the numeric value)
/// </summary>
public class DynamoDbEnumListConverter<TEnum> : IPropertyConverter where TEnum : Enum
{
    public DynamoDBEntry ToEntry(object value)
    {
        if (value is null) return new DynamoDBNull();

        if (value is not IEnumerable<TEnum> list)
            throw new ArgumentException($"Field value is not a list of {typeof(TEnum).Name}. This attribute has been used on a property that is not a list of enum values.");

        return new DynamoDBList(list.Select(x => new Primitive(Enum.GetName(typeof(TEnum), x))));
    }

    public object FromEntry(DynamoDBEntry entry)
    {
        if (entry is null || entry is DynamoDBNull) return null;

        if (entry is not DynamoDBList list)
            throw new ArgumentException("Field value is not a DynamoDBList. This attribute has been used on a property that is not a list of enum values.");

        return list.AsListOfDynamoDBEntry().Select(x =>
                (TEnum)Enum.Parse(typeof(TEnum), x.AsString())).ToList();
    }
}
