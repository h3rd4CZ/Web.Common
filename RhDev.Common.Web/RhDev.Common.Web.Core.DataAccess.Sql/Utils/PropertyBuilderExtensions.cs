using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
#nullable disable warnings

namespace RhDev.Common.Web.Core.DataAccess.Sql.Utils
{
    public static class PropertyBuilderExtensions
    {
        public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder, JsonSerializerOptions? options = default) where T : class, new()
        {
            ValueConverter<T, string> converter = new ValueConverter<T, string>
            (
                v => JsonSerializer.Serialize<T>(v, options),
                v => JsonSerializer.Deserialize<T>(v, options) ?? new T()
            );

            ValueComparer<T> comparer = new ValueComparer<T>
            (
                (l, r) => JsonSerializer.Serialize(l, options) == JsonSerializer.Serialize(r, options),
                v => v == null ? 0 : JsonSerializer.Serialize(v, options).GetHashCode(),
                v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, options), options)
            );

            propertyBuilder.HasConversion(converter);
            propertyBuilder.Metadata.SetValueConverter(converter);
            propertyBuilder.Metadata.SetValueComparer(comparer);
            propertyBuilder.HasColumnType("NVARCHAR(MAX)");

            return propertyBuilder;
        }

        public static PropertyBuilder<List<string>> HasStringListConversion(this PropertyBuilder<List<string>> propertyBuilder, JsonSerializerOptions? options = default)
        {
            ValueConverter<List<string>, string> converter = new ValueConverter<List<string>, string>(
               v => JsonSerializer.Serialize(v, options),
               v => string.IsNullOrEmpty(v) ? default : JsonSerializer.Deserialize<List<string>>(v, options));

            ValueComparer<List<string>> comparer = new ValueComparer<List<string>>(
                (l, r) => JsonSerializer.Serialize(l, options) == JsonSerializer.Serialize(r, options),
                v => v == null ? 0 : JsonSerializer.Serialize(v, options).GetHashCode(),
                v => JsonSerializer.Deserialize<List<string>>(JsonSerializer.Serialize(v, options), options));

            propertyBuilder.HasConversion(converter);
            propertyBuilder.Metadata.SetValueConverter(converter);
            propertyBuilder.Metadata.SetValueComparer(comparer);
            return propertyBuilder;
        }
    }
}
