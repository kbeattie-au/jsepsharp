using JsepSharp.SyntaxTree;
using System.Text.Json.Serialization;

namespace JsepSharp.Plugins.SyntaxTree
{
    public sealed class TernaryNode : SyntaxNode
    {
        const string TYPE_NAME = "ConditionalExpression";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(TernaryNode), TYPE_NAME);

        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        public TernaryNode() : base() { }

        public TernaryNode(SyntaxNode? test, SyntaxNode? consequent, SyntaxNode? alternate) : base()
        {
            Test = test;
            Consequent = consequent;
            Alternate = alternate;
        }

        public SyntaxNode? Test { get; set; }
        public SyntaxNode? Consequent { get; set; }
        public SyntaxNode? Alternate { get; set; }

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

        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        public bool Equals(TernaryNode node)
        {
            return Equals(Test, node.Test) &&
                   Equals(Consequent, node.Consequent) &&
                   Equals(Alternate, node.Alternate);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Test, Consequent, Alternate);
        }
    }
}
