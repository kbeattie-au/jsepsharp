using JsepSharp.Extensions;
using JsepSharp.SyntaxTree;
using Newtonsoft.Json;

namespace JsepSharp.Plugins.SyntaxTree
{
    /// <summary>
    /// Represents an constructor call.
    /// </summary>
    public sealed class NewNode : SyntaxNode
    {
        const string TYPE_NAME = "NewExpression";

        /// <summary>
        /// Node type identifier.
        /// </summary>
        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(NewNode), TYPE_NAME);

        /// <inheritdoc />
        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        /// <summary>
        /// Initialize a new node.
        /// </summary>
        public NewNode() : base()
        {
            Arguments = [];
        }

        /// <summary>
        /// Initialize a new node with parameters.
        /// </summary>
        /// <param name="callee">Node representing the constructor to invoke.</param>
        /// <param name="arguments">Nodes representing the arguments supplied to the constructor.</param>
        public NewNode(SyntaxNode? callee, List<SyntaxNode?> arguments) : base()
        {
            Callee = callee;
            Arguments = arguments;
        }

        /// <summary>
        /// Initialize a new node from a call node.
        /// </summary>
        /// <param name="node">The call node to copy properties from.</param>
        public NewNode(CallNode node)
        {
            Arguments = node.Arguments;
            Callee = node.Callee;
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("New(");
            sb.Node(Callee);
            sb.Delim();
            sb.NodeArray(Arguments);
            sb.OptionalArgument(Optional);
            sb.End();
        }

        /// <summary>
        /// The class being created.
        /// </summary>
        public SyntaxNode? Callee { get; set; }

        /// <summary>
        /// Constructor arguments.
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
        public bool Equals(NewNode node)
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
