using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System;
using System.Globalization;

namespace Hackney.Core.DynamoDb.Converters;

/// <summary>
/// Converter for DateTime objects because the default handling expects the data time string to alays be in a very specific
/// format and will throw an exception if not.
/// </summary>
public class DynamoDbDateTimeConverter : IPropertyConverter
{
    public static readonly string DATEFORMAT = "yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ";

    public DynamoDBEntry ToEntry(object value)
    {
        if (value is null) return new DynamoDBNull();

        return new Primitive
        {
            Value = ((DateTime)value).ToUniversalTime().ToString(DATEFORMAT, CultureInfo.InvariantCulture)
        };
    }

    public object FromEntry(DynamoDBEntry entry)
    {
        if (entry is not Primitive primitive) return (DateTime?)null;

        var dtString = Convert.ToString(primitive.Value, CultureInfo.CurrentCulture);
        return DateTime.Parse(dtString, CultureInfo.CurrentCulture, DateTimeStyles.RoundtripKind);
    }
}
