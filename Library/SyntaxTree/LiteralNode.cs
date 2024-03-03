using JsepNet.Json;
using Newtonsoft.Json;

namespace JsepNet.SyntaxTree
{
    public sealed class LiteralNode : SyntaxNode
    {
        const string TYPE_NAME = "Literal";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(LiteralNode), TYPE_NAME);

        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        public LiteralNode() : base() { }

        public LiteralNode(object? value, string? raw) : base()
        {
            Value = value;
            Raw = raw;
        }

        public LiteralNode(object? value, string? raw, bool optional) : this(value, raw)
        {
            Optional = optional;
        }

        /// <summary>
        /// Converts numeric typed non-double scalar values to doubles.
        /// </summary>
        /// <remarks>
        /// JavaScript numbers are IEEE 754 double-precision floating point values.
        /// </remarks>
        /// <param name="v">Value to potentially convert.</param>
        /// <returns>Scalar value.</returns>
        public static object? NumberToDouble(object? v)
        {
            if (v is double) return v;
            if (v is null) return null;

            Type vt = v.GetType();
            Type? nt = Nullable.GetUnderlyingType(vt);
            Type et = nt ?? vt;

            if (et == typeof(int) || et == typeof(uint) ||
                et == typeof(long) || et == typeof(ulong) ||
                et == typeof(decimal) || et == typeof(float) ||
                et == typeof(short) || et == typeof(ushort) ||
                et == typeof(byte) || et == typeof(sbyte) ||
                et == typeof(Int128) || et == typeof(UInt128))
            {
                return Convert.ToDouble(v);
            }

            return v;
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("Lit");
            sb.Any(Value);
            sb.Delim();
            sb.EscapedText(Raw);
            sb.OptionalArgument(Optional);
            sb.End();
        }

        private object? _val;
        public object? Value
        {
            get => _val;
            set => _val = NumberToDouble(value);
        }

        public string? Raw { get; set; }

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
        public bool Equals(LiteralNode node)
        {
            if (Raw != node.Raw) return false;

            var nodeValue = node.Value;
            if (ReferenceEquals(Value, nodeValue)) return true;

            // Note: The parser uses Doubles for all numbers (like JavaScript),
            // so if you manually make a LiteralNode with an integer for things
            // like tests, equality will fail since this expects strict equality!
            //
            // Considered EqualityComparer<T> and type conversions,
            // but decided to keep things simpler and more efficient.
            return Value is not null && Value.Equals(nodeValue);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Raw, Value);
        }
    }
}
