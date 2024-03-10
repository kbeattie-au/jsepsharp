using JsepSharp.SyntaxTree;
using Newtonsoft.Json;

namespace JsepSharp.Plugins.SyntaxTree
{
    /// <summary>
    /// Represents an assignment expression.
    /// </summary>
    public sealed class AssignmentNode : SyntaxNode
    {
        const string TYPE_NAME = "AssignmentExpression";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(AssignmentNode), TYPE_NAME);

        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        public AssignmentNode() : base() { }

        public AssignmentNode(string? @operator, SyntaxNode? left, SyntaxNode? right) : base()
        {
            Operator = @operator;
            Left = left;
            Right = right;
        }

        /// <summary>
        /// Operator being used for assignment.
        /// </summary>
        public string? Operator { get; set; }

        /// <summary>
        /// The target of assignment.
        /// </summary>
        public SyntaxNode? Left { get; set; }

        /// <summary>
        /// The expression to assign to the target.
        /// </summary>
        public SyntaxNode? Right { get; set; }

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
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("Assign");
            sb.EscapedText(Operator);
            sb.Delim();
            sb.Node(Left);
            sb.Delim();
            sb.Node(Right);
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
        public bool Equals(AssignmentNode node)
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
