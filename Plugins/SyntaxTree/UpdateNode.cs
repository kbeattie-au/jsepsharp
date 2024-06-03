using JsepSharp.SyntaxTree;
using Newtonsoft.Json;

namespace JsepSharp.Plugins.SyntaxTree
{
    /// <summary>
    /// Represents an update expression.
    /// </summary>
    /// <remarks>
    /// This exists primarily for <c>a++</c> and <c>--a</c> unary postfix/prefix increment/decrement operations.
    /// </remarks>
    public sealed class UpdateNode : SyntaxNode, IHasOperator
    {
        const string TYPE_NAME = "UpdateExpression";

        /// <summary>
        /// Node type identifier.
        /// </summary>
        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(UpdateNode), TYPE_NAME);

        /// <inheritdoc />
        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        /// <summary>
        /// Initialize an update node.
        /// </summary>
        public UpdateNode() : base() { }

        /// <summary>
        /// Initialize an update node with parameters.
        /// </summary>
        /// <param name="operator">Operator name.</param>
        /// <param name="argument">Argument node being operated on.</param>
        /// <param name="prefix">Whether operator is prefix or postfix.</param>
        public UpdateNode(string? @operator, SyntaxNode? argument, bool prefix) : base()
        {
            Operator = @operator;
            Argument = argument;
            Prefix = prefix;
        }

        /// <inheritdoc />
        public string? Operator { get; set; }

        /// <summary>
        /// Subject of update assignment.
        /// </summary>
        public SyntaxNode? Argument { get; set; }

        /// <summary>
        /// Whether update assignment is prefix (increment) or postfix (decrement).
        /// </summary>
        public bool Prefix { get; set; }

        /// <summary>
        /// Whether prefix should be serialized in the output JSON.
        /// </summary>
        /// <returns>True if serialized; Otherwise false.</returns>
        public bool ShouldSerializePrefix()
        {
            return Prefix;
        }

        /// <inheritdoc />
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            if (Argument is not null) yield return Argument;
        }

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
            sb.Start("Update");
            sb.EscapedText(Operator);
            sb.Delim();
            sb.Node(Argument);
            sb.Delim();
            sb.Bool(Prefix);
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
        public bool Equals(UpdateNode node)
        {
            return Prefix == Prefix &&
                   Operator == Operator &&
                   Equals(Argument, node.Argument);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Prefix, Operator, Argument);
        }
    }
}
