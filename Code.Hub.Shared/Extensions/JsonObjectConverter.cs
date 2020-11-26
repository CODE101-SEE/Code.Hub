using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Code.Hub.Shared.Extensions
{
    /// <summary>
    /// Custom JsonConverter for System.Text.Json.Serialization, which helps in deserialize when model has object inside, like dictionary string,object
    /// https://github.com/dotnet/runtime/issues/31408
    /// </summary>
    public class JsonObjectConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    var list = new List<object>();
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        list.Add(ExtractValue(ref reader));
                    }
                    return list.ToArray();
                }

                return ExtractValue(ref reader);
            }
            catch (Exception)
            {
                // Use JsonElement as fallback.
                using JsonDocument document = JsonDocument.ParseValue(ref reader);
                return document.RootElement.Clone();
            }
        }

        private object ExtractValue(ref Utf8JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.True)
            {
                return true;
            }

            if (reader.TokenType == JsonTokenType.False)
            {
                return false;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt64(out long longNumber))
                {
                    return longNumber;
                }

                if (reader.TryGetInt32(out int intNumber))
                {
                    return intNumber;
                }

                return reader.GetDouble();
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                if (reader.TryGetDateTime(out DateTime datetime))
                {
                    return datetime;
                }

                return reader.GetString();
            }

            throw new JsonException($"'{reader.TokenType}' is not supported");
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            throw new InvalidOperationException("Write operation not supported");
        }
    }
}
