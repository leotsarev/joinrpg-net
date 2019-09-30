// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
using System;
using System.Linq;
using Newtonsoft.Json;

namespace PscbApi
{
    /// <summary>
    /// Must be used for enums with values that marked with <see cref="IdentifierAttribute"/>
    /// </summary>
    internal class IdentifiableEnumConverter : JsonConverter
    {
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                if (value.GetType().IsEnum)
                    writer.WriteValue(value as Enum);
                else
                    writer.WriteValue(value.ToString());
            }
            else
                writer.WriteNull();
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var nullable = false;
            Type actualType = objectType;
            if (!objectType.IsEnum)
            {
                actualType = Nullable.GetUnderlyingType(objectType);
                if (!actualType?.IsEnum ?? false)
                    actualType = null;
                nullable = actualType != null;
            }

            if (actualType != null)
            {
                if (existingValue != null)
                {
                    object result = Enum.GetValues(actualType)
                        .Cast<Enum>()
                        .FirstOrDefault(v => v.GetIdentifier().Equals(existingValue));
                    if (result != null)
                        return result;
                }
                else if (nullable)
                    return null;
            }

            throw new InvalidCastException($"\"{existingValue}\" could not be converted to {actualType}");
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
            => objectType.IsEnum;
    }
}
