using JsepSharp.SyntaxTree;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsepSharp.Json
{
    /// <summary>
    /// Parses a SyntaxNode object to the correct subclass, using the type property.
    /// </summary>
    public sealed class SyntaxNodeConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanWrite => false;

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return typeof(SyntaxNode).IsAssignableFrom(objectType);
        }

        /// <inheritdoc />
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            var jObject = JObject.Load(reader);
            var sourceType = jObject["type"]?.Value<string>() ?? "";
            object target;

            target = Jsep.NodeTypesByStrings.TryGetValue(sourceType, out var nodeType) ?
                Activator.CreateInstance(nodeType)! :
                new UnknownNode() { UnknownType = sourceType };

            if (target is not null)
            {
                serializer.Populate(jObject.CreateReader(), target);
            }

            return target;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
