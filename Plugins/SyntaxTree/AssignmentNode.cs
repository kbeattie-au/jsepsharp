using JsepSharp.SyntaxTree;
using System.Text.Json.Serialization;

namespace JsepSharp.Plugins.SyntaxTree
{
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

        public string? Operator { get; set; }
        public SyntaxNode? Left { get; set; }
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

        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        public bool Equals(AssignmentNode node)
        {
            return Operator == node.Operator &&
                   Equals(Left, node.Left) &&
                   Equals(Right, node.Right);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Operator, Left, Right);
        }
    }
}
