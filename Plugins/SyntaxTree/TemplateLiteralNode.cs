using JsepNet.SyntaxTree;
using System.Text.Json.Serialization;

namespace JsepNet.Plugins.SyntaxTree
{
    public sealed class TemplateLiteralNode : SyntaxNode
    {
        const string TYPE_NAME = "TemplateLiteral";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(TemplateLiteralNode), TYPE_NAME);

        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        public TemplateLiteralNode() : base()
        {
            Quasis = [];
            Expressions = [];
        }

        public TemplateLiteralNode(List<TemplateElement?> quasis, List<SyntaxNode?> expressions) : base()
        {
            Quasis = quasis;
            Expressions = expressions;
        }

        public List<TemplateElement?> Quasis { get; set; }
        public List<SyntaxNode?> Expressions { get; set; }

        public override void ReplaceNodes(NodeReplacer searcher)
        {
            // TODO:
            // Skipping Quasis since they MUST be TemplateElement.
            // Also, this code may change since that plug-in isn't written yet.

            for (int i = 0; i < Expressions.Count; i++)
            {
                var node = Expressions[i];
                if (searcher(node, out var nodeOut))
                {
                    Expressions[i] = nodeOut;
                    nodeOut?.ReplaceNodes(searcher);
                }
            }
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("TElem");
            sb.NodeArray(Quasis);
            sb.Delim();
            sb.NodeArray(Expressions);
            sb.End();
        }

        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        public bool Equals(TemplateLiteralNode node)
        {
            return SequenceEquals(Quasis, node.Quasis) &&
                SequenceEquals(Expressions, node.Expressions);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Quasis, Expressions);
        }
    }
}
