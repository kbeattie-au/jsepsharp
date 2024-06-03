using JsepSharp.SyntaxTree;
using Newtonsoft.Json;

namespace JsepSharp.Plugins.SyntaxTree
{
    /// <summary>
    /// Represents a ternary/conditional expression.
    /// </summary>
    public sealed class TernaryNode : SyntaxNode
    {
        const string TYPE_NAME = "ConditionalExpression";

        /// <summary>
        /// Node type identifier.
        /// </summary>
        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(TernaryNode), TYPE_NAME);

        /// <inheritdoc />
        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        /// <summary>
        /// Initialize a ternary/conditional node.
        /// </summary>
        public TernaryNode() : base() { }

        /// <summary>
        /// Initialize a ternary/conditional node with parameters.
        /// </summary>
        /// <param name="test">Test clause node.</param>
        /// <param name="consequent">Node to use when test clause criteria is met.</param>
        /// <param name="alternate">Node to use when test clause criteria is not met.</param>
        public TernaryNode(SyntaxNode? test, SyntaxNode? consequent, SyntaxNode? alternate) : base()
        {
            Test = test;
            Consequent = consequent;
            Alternate = alternate;
        }

        /// <summary>
        /// Test criteria.
        /// </summary>
        public SyntaxNode? Test { get; set; }

        /// <summary>
        /// If test criteria is true.
        /// </summary>
        public SyntaxNode? Consequent { get; set; }

        /// <summary>
        /// If test criteria is false.
        /// </summary>
        public SyntaxNode? Alternate { get; set; }

        /// <inheritdoc />
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            if (Test is not null) { yield return Test; }
            if (Consequent is not null) { yield return Consequent; }
            if (Alternate is not null) { yield return Alternate; }
        }

        /// <inheritdoc />
        public override void ReplaceNodes(NodeReplacer searcher)
        {
            if (searcher(Test, out var outTest))
            {
                Test = outTest;
                outTest?.ReplaceNodes(searcher);
            }

            if (searcher(Consequent, out var outConsequent))
            {
                Consequent = outConsequent;
                outConsequent?.ReplaceNodes(searcher);
            }

            if (searcher(Alternate, out var outAlternate))
            {
                Alternate = outAlternate;
                outAlternate?.ReplaceNodes(searcher);
            }
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("IIf");
            sb.Node(Test);
            sb.Delim();
            sb.Node(Consequent);
            sb.Delim();
            sb.Node(Alternate);
            sb.End();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        /// <summary>
        /// Determines if another node of the same type has the same test, consequent, and alternate as this one.
        /// </summary>
        /// <param name="node">The node to compare with the current one.</param>
        /// <returns><c>true</c> if the specified node is equal to the current one; else <c>false</c>.</returns>
        public bool Equals(TernaryNode node)
        {
            return Equals(Test, node.Test) &&
                   Equals(Consequent, node.Consequent) &&
                   Equals(Alternate, node.Alternate);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Test, Consequent, Alternate);
        }
    }
}
