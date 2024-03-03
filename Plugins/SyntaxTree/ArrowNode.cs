using JsepNet.SyntaxTree;
using System.Text.Json.Serialization;

namespace JsepNet.Plugins.SyntaxTree
{
    public sealed class ArrowNode : SyntaxNode
    {
        const string TYPE_NAME = "ArrowFunctionExpression";

        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(ArrowNode), TYPE_NAME);

        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        public ArrowNode() : base()
        {
            Params = [];
        }

        public ArrowNode(SyntaxNode? body) : this()
        {
            Body = body;
        }

        public ArrowNode(List<SyntaxNode?> @params, SyntaxNode? body) : base()
        {
            Params = @params;
            Body = body;
        }

        public ArrowNode(List<SyntaxNode?> @params, SyntaxNode? body, bool @async) : this(@params, body)
        {
            Async = @async;
        }

        public List<SyntaxNode?> Params { get; set; }

        public SyntaxNode? Body { get; set; }

        public bool Async { get; set; }

        public bool ShouldSerializeAsync()
        {
            return Async;
        }

        /// <inheritdoc />
        public override void ReplaceNodes(NodeReplacer searcher)
        {
            if (searcher(Body, out var outBody))
            {
                Body = outBody;
                outBody?.ReplaceNodes(searcher);
            }

            for (int i = 0; i < Params.Count; i++)
            {
                var node = Params[i];
                if (searcher(node, out var nodeOut))
                {
                    Params[i] = nodeOut;
                    nodeOut?.ReplaceNodes(searcher);
                }
            }
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start(Async ? "AsyncArrow" : "Arrow");
            sb.NodeArray(Params);
            sb.Delim();
            sb.Node(Body);
            sb.End();
        }

        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        public bool Equals(ArrowNode node)
        {
            return Equals(Body, node.Body) &&
                Equals(Async, node.Async) &&
                SequenceEquals(Params, node.Params);   
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Body, Params);
        }
    }
}
