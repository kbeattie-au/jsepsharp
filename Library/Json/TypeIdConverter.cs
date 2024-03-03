using Newtonsoft.Json;

namespace JsepNet.Json
{
    /// <summary>
    /// Converts TypeId to Type, when the JSON is created.
    /// </summary>
    [Obsolete("Remove this from the code. It ended up becoming unused.")]
    internal class TypeIdConverter : JsonConverter<int>
    {
        /// <inheritdoc />
        public override int ReadJson(JsonReader reader, Type objectType, int existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // This is never read. It is an output only conversion.
            return 0;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, int value, JsonSerializer serializer)
        {
            if (Jsep.NodeNamesByTypeIds.TryGetValue(value, out string? name))
            {
                writer.WriteValue(name);
                return;
            }

            writer.WriteNull();
        }
    }
}
