﻿using Newtonsoft.Json;

namespace JsepSharp.SyntaxTree
{
    /// <summary>
    /// Represents an identifier.
    /// </summary>
    public sealed class IdentifierNode : SyntaxNode
    {
        const string TYPE_NAME = "Identifier";

        /// <summary>
        /// Node type identifier.
        /// </summary>
        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(IdentifierNode), TYPE_NAME);

        /// <inheritdoc />
        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        /// <summary>
        /// Initialize an identiifer node.
        /// </summary>
        public IdentifierNode() : base() { }

        /// <summary>
        /// Initialize an identifier node with parameters.
        /// </summary>
        /// <param name="name">Identifier name.</param>
        public IdentifierNode(string? name) : base()
        {
            Name = name;
        }

        /// <summary>
        /// Initialize an identifier node with parameters.
        /// </summary>
        /// <param name="name">Identifier name.</param>
        /// <param name="optional">Whether or not an optional indicator (?) was supplied.</param>
        public IdentifierNode(string? name, bool optional) : this(name)
        {
            Optional = optional;
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("Id");
            sb.EscapedText(Name);
            sb.OptionalArgument(Optional);
            sb.End();
        }

        /// <summary>Identifier name.</summary>
        public string? Name { get; set; }

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
        public bool Equals(IdentifierNode node)
        {
            return Name == node.Name;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }
    }
}
