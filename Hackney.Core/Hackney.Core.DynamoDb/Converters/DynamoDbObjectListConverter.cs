using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hackney.Core.DynamoDb.Converters;

/// <summary>
/// Converter for a list of sub-objects.
/// Treats a custom sub-objects as straight Json, meaning any DynamoDb attributes it may have are not applied
/// Will (de)serialise any enum properties as the name value (not the numeric value)
/// </summary>
public class DynamoDbObjectListConverter<T> : IPropertyConverter
{
    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    public DynamoDBEntry ToEntry(object value)
    {
        if (value is null) return new DynamoDBNull();

        if (value is not IEnumerable<T> list)
            throw new ArgumentException($"Field value is not a list of {typeof(T).Name}. This attribute has been used on a property that is not a list of custom objects.");

        return new DynamoDBList(list.Select(x => Document.FromJson(JsonSerializer.Serialize(x, CreateJsonOptions()))));
    }

    public object FromEntry(DynamoDBEntry entry)
    {
        if (entry is null || entry is DynamoDBNull) return null;

        if (entry is not DynamoDBList list)
            throw new ArgumentException("Field value is not a DynamoDBList. This attribute has been used on a property that is not a list of custom objects.");

        return list.AsListOfDocument().Select(x => JsonSerializer.Deserialize<T>(x.ToJson(), CreateJsonOptions())).ToList();
    }
}
