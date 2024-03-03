using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsepNet.Json
{
    /// <summary>
    /// Converter that assures scalar values, if numbers, are always converted to doubles.
    /// </summary>
    /// <remarks>
    /// Important: Only use this with properties where object should only be simple scalar values.
    /// </remarks>
    [Obsolete("Remove this from the code. It ended up becoming unused.")]
    public class NumbersAlwaysDoublesScalarConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(object);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);

            // Leaves strings, bools, and nulls alone. Converts all numbers to doubles.
            return token.Type switch
            {
                JTokenType.String => token.ToString(),
                JTokenType.Integer or JTokenType.Float => token.ToObject<double>(),
                JTokenType.Boolean => token.Value<bool>(),
                JTokenType.Undefined or JTokenType.Null => null,
                _ => throw new JsonSerializationException($"Unexpected token type: {token.Type}"),
            };
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
