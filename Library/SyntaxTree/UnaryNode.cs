﻿using Newtonsoft.Json;

namespace JsepSharp.SyntaxTree
{
    /// <summary>
    /// Represents an unary operation.
    /// </summary>
    public sealed class UnaryNode : SyntaxNode, IHasOperator
    {
        const string TYPE_NAME = "UnaryExpression";

        /// <summary>
        /// Node type identifier.
        /// </summary>
        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(UnaryNode), TYPE_NAME);

        /// <inheritdoc />
        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        /// <summary>
        /// Initialize a unary operator node.
        /// </summary>
        public UnaryNode() : base() { }

        /// <summary>
        /// Initialize a unary operator node with parameters.
        /// </summary>
        /// <param name="operator">Operator name.</param>
        /// <param name="argument">Node the operator applies to.</param>
        /// <param name="prefix">Whether operator is prefix or postfix.</param>
        public UnaryNode(string? @operator, SyntaxNode? argument, bool prefix) : base()
        {
            Operator = @operator;
            Argument = argument;
            Prefix = prefix;
        }

        /// <inheritdoc />
        public string? Operator { get; set; }

        /// <summary>
        /// The node the operator applies to.
        /// </summary>
        public SyntaxNode? Argument { get; set; }

        /// <summary>
        /// Whether it is a prefix or postfix operation.
        /// </summary>
        public bool Prefix { get; set; }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("Un");
            sb.EscapedText(Operator);
            sb.Delim();
            sb.Node(Argument);
            sb.Delim();
            sb.Bool(Prefix);
            sb.End();
        }

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
        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        /// <summary>
        /// Determines if another node of the same type has the same values as this one.
        /// </summary>
        /// <param name="node">The node to compare with the current one.</param>
        /// <returns><c>true</c> if the specified node is equal to the current one; else <c>false</c>.</returns>
        public bool Equals(UnaryNode node)
        {
            return Prefix == node.Prefix &&
                   Operator == node.Operator &&
                   Equals(Argument, node.Argument);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Operator, Argument, Prefix);
        }
    }
}
