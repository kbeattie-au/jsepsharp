using JsepNet.SyntaxTree;
using System.Text.Json.Serialization;

namespace JsepNet.Plugins.SyntaxTree
{
    public sealed class AwaitNode : SyntaxNode
    {
        const string TYPE_NAME = "AwaitExpression";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(AwaitNode), TYPE_NAME);

        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        public AwaitNode() : base() { }

        public AwaitNode(SyntaxNode? argument) : base()
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
            sb.Start("Await");
            sb.Node(Argument);
            sb.End();
        }

        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        public bool Equals(AwaitNode node)
        {
            return Equals(Argument, node.Argument);
        }

        public override int GetHashCode()
        {
            return Argument?.GetHashCode() ?? 0;
        }
    }
}
