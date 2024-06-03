using JsepSharp.Extensions;
using JsepSharp.SyntaxTree;
using Newtonsoft.Json;

namespace JsepSharp.Plugins.SyntaxTree
{
    /// <summary>
    /// Represents an arrow / lambda function expression.
    /// </summary>
    public sealed class ArrowNode : SyntaxNode
    {
        const string TYPE_NAME = "ArrowFunctionExpression";

        /// <summary>
        /// Node type identifier.
        /// </summary>
        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(ArrowNode), TYPE_NAME);

        /// <inheritdoc />
        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        /// <summary>
        /// Initialize a arrow method node.
        /// </summary>
        public ArrowNode() : base()
        {
            Params = [];
        }

        /// <summary>
        /// Initialize a arrow method node with parameters.
        /// </summary>
        /// <param name="body">Node representing the expression within the arrow method.</param>
        public ArrowNode(SyntaxNode? body) : this()
        {
            Body = body;
        }

        /// <summary>
        /// Initialize a arrow method node with parameters.
        /// </summary>
        /// <param name="params">Nodes representing arguments to the arrow method.</param>
        /// <param name="body">Node representing the expression within the arrow method.</param>
        public ArrowNode(List<SyntaxNode?> @params, SyntaxNode? body) : base()
        {
            Params = @params;
            Body = body;
        }

        /// <summary>
        /// Initialize a arrow method node with parameters.
        /// </summary>
        /// <param name="params">Nodes representing arguments to the arrow method.</param>
        /// <param name="body">Node representing the expression within the arrow method.</param>
        /// <param name="async">Indicates whether the arrow method is async.</param>
        public ArrowNode(List<SyntaxNode?> @params, SyntaxNode? body, bool @async) : this(@params, body)
        {
            Async = @async;
        }

        /// <summary>
        /// Arguments for arrow function.
        /// </summary>
        public List<SyntaxNode?> Params { get; set; }

        /// <summary>
        /// Body of arrow function.
        /// </summary>
        public SyntaxNode? Body { get; set; }

        /// <summary>
        /// Whether or not this arrow function is async.
        /// </summary>
        /// <remarks>
        /// Requires that AsyncAwaitPlugin is loaded to operate properly.
        /// </remarks>
        public bool Async { get; set; }

        /// <summary>
        /// Whether async should be serialized in the output JSON.
        /// </summary>
        /// <returns>True if serialized; Otherwise false.</returns>
        public bool ShouldSerializeAsync()
        {
            return Async;
        }

        /// <inheritdoc />
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            if (Body is not null) yield return Body;

            foreach (var a in Params.Compact())
            {
                yield return a;
            }
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
        public bool Equals(ArrowNode node)
        {
            return Equals(Body, node.Body) &&
                Equals(Async, node.Async) &&
                SequenceEquals(Params, node.Params);   
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Body, Params);
        }
    }
}
