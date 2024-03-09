using JsepSharp.SyntaxTree;
using System.Text.Json.Serialization;

namespace JsepSharp.Plugins.SyntaxTree
{
    public sealed class ObjectProperty : SyntaxNode
    {
        const string TYPE_NAME = "Property";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(ObjectProperty), TYPE_NAME);

        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        public ObjectProperty() : base() { }

        public ObjectProperty(bool computed, SyntaxNode? key, SyntaxNode? value, bool shorthand)
        {
            Computed = computed;
            Key = key;
            Value = value;
            Shorthand = shorthand;
        }

        public bool Computed { get; set; }
        public SyntaxNode? Key { get; set; }
        public SyntaxNode? Value { get; set; }
        public bool Shorthand { get; set; }

        public bool ShouldSerializeComputed()
        {
            return Computed;
        }

        public bool ShouldSerializeShorthand()
        {
            return Shorthand;
        }

        /// <inheritdoc />
        public override void ReplaceNodes(NodeReplacer searcher)
        {
            if (searcher(Key, out var outKey))
            {
                Key = outKey;
                outKey?.ReplaceNodes(searcher);
            }

            if (searcher(Value, out var outValue))
            {
                Value = outValue;
                outValue?.ReplaceNodes(searcher);
            }
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("Prop");
            sb.Bool(Computed);
            sb.Delim();
            sb.Node(Key);
            sb.Delim();
            sb.Node(Value);
            sb.Delim();
            sb.Bool(Shorthand);
            sb.End();
        }

        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        public bool Equals(ObjectProperty node)
        {
            return Computed == node.Computed &&
                   Shorthand == node.Shorthand &&
                   Equals(Key, node.Key) &&
                   Equals(Value, node.Value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Computed, Shorthand, Key, Value);
        }
    }
}
