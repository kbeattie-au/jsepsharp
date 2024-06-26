﻿using JsepSharp.SyntaxTree;
using Newtonsoft.Json;

namespace JsepSharp.Plugins.SyntaxTree
{
    /// <summary>
    /// Represents an await statement.
    /// </summary>
    public sealed class AwaitNode : SyntaxNode
    {
        const string TYPE_NAME = "AwaitExpression";

        /// <summary>
        /// Node type identifier.
        /// </summary>
        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(AwaitNode), TYPE_NAME);

        /// <inheritdoc />
        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        /// <summary>
        /// Initialize an await node.
        /// </summary>
        public AwaitNode() : base() { }

        /// <summary>
        /// Initialize an await node with parameters.
        /// </summary>
        /// <param name="argument">The node being awaited.</param>
        public AwaitNode(SyntaxNode? argument) : base()
        {
            Argument = argument;
        }

        /// <summary>
        /// The node being awaited.
        /// </summary>
        public SyntaxNode? Argument { get; set; }

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
            sb.Start("Await");
            sb.Node(Argument);
            sb.End();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        /// <summary>
        /// Determines if another node of the same type has the same argument as this one.
        /// </summary>
        /// <param name="node">The node to compare with the current one.</param>
        /// <returns><c>true</c> if the specified node is equal to the current one; else <c>false</c>.</returns>
        public bool Equals(AwaitNode node)
        {
            return Equals(Argument, node.Argument);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Argument?.GetHashCode() ?? 0;
        }
    }
}
