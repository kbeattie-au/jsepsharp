using JsepSharp.SyntaxTree;
using System.Text.Json.Serialization;

namespace JsepSharp.Plugins.SyntaxTree
{
    public sealed class SpreadNode : SyntaxNode
    {
        const string TYPE_NAME = "SpreadElement";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(SpreadNode), TYPE_NAME);

        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        public SpreadNode() : base() { }

        public SpreadNode(SyntaxNode? argument) : base()
        {
            Argument = argument;
        }

        public SyntaxNode? Argument { get; set; }

        /// <inheritdoc />
        public override void ReplaceNodes(NodeReplacer searcher)
        {
            if (searcher(Argument, out var outArg))
            {
                Argument = outArg;
                outArg?.ReplaceNodes(searcher);
            }
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("Spread");
            sb.Node(Argument);
            sb.End();
        }

        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        public bool Equals(SpreadNode node)
        {
            return Equals(Argument, node.Argument);
        }

        public override int GetHashCode()
        {
            return Argument?.GetHashCode() ?? 0;
        }
    }
}
