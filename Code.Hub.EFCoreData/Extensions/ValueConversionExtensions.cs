using Code.Hub.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Code.Hub.EFCoreData.Extensions
{
    public static class ValueConversionExtensions
    {
        public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder) where T : class, new()
        {
            ValueConverter<T, string> converter = new ValueConverter<T, string>
            (
                model => JsonSerializationExtensions.Serialize(model),
                valueFromDb => JsonSerializationExtensions.Deserialize<T>(valueFromDb) ?? new T()
            );

            ValueComparer<T> comparer = new ValueComparer<T>
            (
                (leftModel, rightModel) => JsonSerializationExtensions.Serialize(leftModel) == JsonSerializationExtensions.Serialize(rightModel),
                model => model == null ? 0 : JsonSerializationExtensions.Serialize(model).GetHashCode(),
                model => JsonSerializationExtensions.Deserialize<T>(JsonSerializationExtensions.Serialize(model))
            );

            propertyBuilder.HasConversion(converter);
            propertyBuilder.Metadata.SetValueConverter(converter);
            propertyBuilder.Metadata.SetValueComparer(comparer);

            return propertyBuilder;
        }
    }
}
