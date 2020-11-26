using System.Text.Json;

namespace Code.Hub.Shared.Extensions
{
    public static class JsonSerializationExtensions
    {
        static readonly JsonSerializerOptions DefaultSerializerOptions = new JsonSerializerOptions();

        static JsonSerializationExtensions()
        {
            DefaultSerializerOptions.Converters.Add(new JsonObjectConverter());
        }

        public static string Serialize(object obj)
        {
            return Serialize(obj, DefaultSerializerOptions);
        }

        public static string Serialize(object obj, JsonSerializerOptions options)
        {
            return JsonSerializer.Serialize(obj, options);
        }

        public static T Deserialize<T>(string message)
        {
            return JsonSerializer.Deserialize<T>(message, DefaultSerializerOptions);
        }

        public static T Deserialize<T>(string message, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<T>(message, options);
        }
    }
}
