using JsepSharp.SyntaxTree;
using Newtonsoft.Json;

namespace JsepSharp.Plugins.SyntaxTree
{
    /// <summary>
    /// Represents a property (key/value) belonging to an object.
    /// </summary>
    public sealed class ObjectProperty : SyntaxNode
    {
        const string TYPE_NAME = "Property";

        /// <summary>
        /// Node type identifier.
        /// </summary>
        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(ObjectProperty), TYPE_NAME);

        /// <inheritdoc />
        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        /// <summary>
        /// Initialize an object property node.
        /// </summary>
        public ObjectProperty() : base() { }

        /// <summary>
        /// Initialize an object property node with parameters.
        /// </summary>
        /// <param name="computed">Whether or not brackets were used to access the member.</param>
        /// <param name="key">Property key.</param>
        /// <param name="value">Property value.</param>
        /// <param name="shorthand">Whether this represents a shorthand.</param>
        public ObjectProperty(bool computed, SyntaxNode? key, SyntaxNode? value, bool shorthand)
        {
            Computed = computed;
            Key = key;
            Value = value;
            Shorthand = shorthand;
        }

        /// <summary>
        /// Indicates a calculated property key (brackets).
        /// </summary>
        public bool Computed { get; set; }

        /// <summary>
        /// The property key.
        /// </summary>
        public SyntaxNode? Key { get; set; }

        /// <summary>
        /// The property value.
        /// </summary>
        public SyntaxNode? Value { get; set; }

        /// <summary>
        /// Indiciates a property value shorthand syntax was used.
        /// </summary>
        public bool Shorthand { get; set; }

        /// <summary>
        /// Whether computed should be serialized in the output JSON.
        /// </summary>
        /// <returns>True if serialized; Otherwise false.</returns>
        public bool ShouldSerializeComputed()
        {
            return Computed;
        }

        /// <summary>
        /// Whether shorthand should be serialized in the output JSON.
        /// </summary>
        /// <returns>True if serialized; Otherwise false.</returns>
        public bool ShouldSerializeShorthand()
        {
            return Shorthand;
        }

        /// <inheritdoc />
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            if (Key is not null) { yield return Key; }
            if (Value is not null) { yield return Value; }
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

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        /// <summary>
        /// Determines if another node of the same type has the same values as this one.
        /// </summary>
        /// <param name="node">The node to compare with the current one.</param>
        /// <returns><c>true</c> if the specified node is equal to the current one; else <c>false</c>.</returns>
        public bool Equals(ObjectProperty node)
        {
            return Computed == node.Computed &&
                   Shorthand == node.Shorthand &&
                   Equals(Key, node.Key) &&
                   Equals(Value, node.Value);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Computed, Shorthand, Key, Value);
        }
    }
}
