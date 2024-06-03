using JsepSharp.Extensions;
using Newtonsoft.Json;

namespace JsepSharp.SyntaxTree
{
    /// <summary>
    /// Represents a function call.
    /// </summary>
    public sealed class CallNode : SyntaxNode
    {
        const string TYPE_NAME = "CallExpression";

        /// <summary>
        /// Node type identifier.
        /// </summary>
        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(CallNode), TYPE_NAME);

        /// <inheritdoc />
        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        /// <summary>
        /// Initialize a call node.
        /// </summary>
        public CallNode() : base()
        {
            Arguments = [];
        }

        /// <summary>
        /// Initialize a call node with parameters.
        /// </summary>
        /// <param name="callee">Node representing the function to invoke.</param>
        /// <param name="arguments">Nodes representing the arguments supplied to the function.</param>
        public CallNode(SyntaxNode? callee, List<SyntaxNode?> arguments) : base()
        {
            Callee = callee;
            Arguments = arguments;
        }

        /// <summary>
        /// Initialize a call node with parameters.
        /// </summary>
        /// <param name="callee">Node representing the function to invoke.</param>
        /// <param name="arguments">Nodes representing the arguments supplied to the function.</param>
        /// <param name="optional">Whether or not an optional indicator (?) was supplied.</param>
        public CallNode(SyntaxNode? callee, List<SyntaxNode?> arguments, bool optional) : this(callee, arguments)
        {
            Optional = optional;
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("Call");
            sb.Node(Callee);
            sb.Delim();
            sb.NodeArray(Arguments);
            sb.OptionalArgument(Optional);
            sb.End();
        }

        /// <summary>
        /// Function being called.
        /// </summary>
        public SyntaxNode? Callee { get; set; }

        /// <summary>
        /// Arguments for function.
        /// </summary>
        public List<SyntaxNode?> Arguments { get; set; }

        /// <inheritdoc />
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            if (Callee is not null) yield return Callee;

            foreach (var a in Arguments.Compact())
            {
                yield return a;
            }
        }

        /// <inheritdoc />
        public override void ReplaceNodes(NodeReplacer searcher)
        {
            if (searcher(Callee, out var outCallee))
            {
                Callee = outCallee;
                outCallee?.ReplaceNodes(searcher);
            }

            for (int i = 0; i < Arguments.Count; i++)
            {
                var node = Arguments[i];
                if (searcher(node, out var nodeOut))
                {
                    Arguments[i] = nodeOut;
                    nodeOut?.ReplaceNodes(searcher);
                }
            }
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        /// <summary>
        /// Determines if another node of the same type has the same values and entries as this one.
        /// </summary>
        /// <param name="node">The node to compare with the current one.</param>
        /// <returns><c>true</c> if the specified node is equal to the current one; else <c>false</c>.</returns>
        public bool Equals(CallNode node)
        {
            return Equals(Callee, node.Callee) &&
                   SequenceEquals(Arguments, node.Arguments);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Callee, Arguments);
        }
    }
}
