using System.Text.Json.Serialization;

namespace JsepNet.SyntaxTree
{
    internal sealed class BinaryOpInfo(string value, double prec, bool rightAssoc) : SyntaxNode()
    {
        const string TYPE_NAME = "BinaryOpInfo";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(BinaryOpInfo), TYPE_NAME);

        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        public string Value { get; set; } = value;

        public double Precision { get; set; } = prec;

        public bool RightAssociative { get; set; } = rightAssoc;

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("OpInfo");
            sb.EscapedText(Value);
            sb.Delim();
            sb.Append(Precision);
            sb.Delim();
            sb.Bool(RightAssociative);
            sb.End();
        }

        /// <summary>
        /// Determines if another node of the same type has the same values as this one.
        /// </summary>
        /// <param name="node">The node to compare with the current one.</param>
        /// <returns><c>true</c> if the specified node is equal to the current one; else <c>false</c>.</returns>
        public bool Equals(BinaryOpInfo node)
        {
            return Value == node.Value &&
                   Precision == node.Precision &&
                   RightAssociative == node.RightAssociative;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Precision, RightAssociative);
        }
    }
}
