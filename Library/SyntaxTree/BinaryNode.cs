using Newtonsoft.Json;

namespace JsepSharp.SyntaxTree
{
    /// <summary>
    /// Represents an binary operation.
    /// </summary>
    public sealed class BinaryNode : SyntaxNode, IHasOperator
    {
        const string TYPE_NAME = "BinaryExpression";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(BinaryNode), TYPE_NAME);

        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        public BinaryNode() : base() { }

        public BinaryNode(string? @operator, SyntaxNode? left, SyntaxNode? right) : base()
        {
            Operator = @operator;
            Left = left;
            Right = right;
        }

        /// <inheritdoc />
        public string? Operator { get; set; }

        /// <summary>
        /// Left-hand side of the operation.
        /// </summary>
        public SyntaxNode? Left { get; set; }

        /// <summary>
        /// Right-hand side of the operation.
        /// </summary>
        public SyntaxNode? Right { get; set; }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("Bin");
            sb.EscapedText(Operator);
            sb.Delim();
            sb.Node(Left);
            sb.Delim();
            sb.Node(Right);
            sb.End();
        }

        /// <inheritdoc />
        public override void ReplaceNodes(NodeReplacer searcher)
        {
            if (searcher(Left, out var outLeft))
            {
                Left = outLeft;
                outLeft?.ReplaceNodes(searcher);
            }

            if (searcher(Right, out var outRight))
            {
                Right = outRight;
                outRight?.ReplaceNodes(searcher);
            }
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
        public bool Equals(BinaryNode node)
        {
            return Operator == node.Operator &&
                   Equals(Left, node.Left) &&
                   Equals(Right, node.Right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Operator, Left, Right);
        }
    }
}
