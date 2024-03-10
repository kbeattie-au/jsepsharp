using Newtonsoft.Json;

namespace JsepSharp.SyntaxTree
{
    /// <summary>
    /// This is used during parsing while using a stack to create the correct
    /// order of operations for binary operators. It should never be returned from the library.
    /// </summary>
    /// <param name="value">Operator.</param>
    /// <param name="prec">Precedence.</param>
    /// <param name="rightAssoc">Right associative?</param>
    internal sealed class BinaryOpInfo(string value, double prec, bool rightAssoc) : SyntaxNode()
    {
        const string TYPE_NAME = "BinaryOpInfo";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(BinaryOpInfo), TYPE_NAME);

        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        /// <summary>
        /// Operator name.
        /// </summary>
        public string Value { get; set; } = value;

        /// <summary>
        /// Precedence of operator.
        /// </summary>
        public double Precision { get; set; } = prec;

        /// <summary>
        /// Whether operator is right-associative.
        /// </summary>
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
            sb.Number(Precision);
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
