﻿using Newtonsoft.Json;

namespace JsepSharp.SyntaxTree
{
    /// <summary>
    /// Represents a member access expression.
    /// </summary>
    public sealed class MemberNode : SyntaxNode
    {
        const string TYPE_NAME = "MemberExpression";

        /// <summary>
        /// Node type identifier.
        /// </summary>
        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(MemberNode), TYPE_NAME);

        /// <inheritdoc />
        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        /// <summary>
        /// Initialize a member access node.
        /// </summary>
        public MemberNode() : base() { }

        /// <summary>
        /// Initialize a member access node with parameters.
        /// </summary>
        /// <param name="computed">Whether or not brackets were used to access the member.</param>
        /// <param name="object">The node representing the object containing the member.</param>
        /// <param name="property">The node representing the member on the object being accessed.</param>
        public MemberNode(bool computed, SyntaxNode? @object, SyntaxNode? property) : base()
        {
            Computed = computed;
            Object = @object;
            Property = property;
        }

        /// <summary>
        /// Initialize a member access node with parameters.
        /// </summary>
        /// <param name="computed">Whether or not brackets were used to access the member.</param>
        /// <param name="object">The node representing the object containing the member.</param>
        /// <param name="property">The node representing the member on the object being accessed.</param>
        /// <param name="optional">Whether or not an optional indicator (?) was supplied.</param>
        public MemberNode(bool computed, SyntaxNode? @object, SyntaxNode? property, bool optional) :
            this(computed, @object, property)
        {
            Optional = optional;
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("Member");
            sb.Bool(Computed);
            sb.Delim();
            sb.Node(Object);
            sb.Delim();
            sb.Node(Property);
            sb.OptionalArgument(Optional);
            sb.End();
        }

        /// <summary>
        /// Whether the property is accessed by brackets (as opposed to dot convention).
        /// </summary>
        public bool Computed { get; set; }

        /// <summary>
        /// Node containing the member being accessed.
        /// </summary>
        public SyntaxNode? Object { get; set; }

        /// <summary>
        /// The member being accessed.
        /// </summary>
        public SyntaxNode? Property { get; set; }

        /// <summary>
        /// Whether computed should be serialized in the output JSON.
        /// </summary>
        /// <returns>True if serialized; Otherwise false.</returns>
        public bool ShouldSerializeComputed()
        {
            return Computed;
        }

        /// <inheritdoc />
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            if (Object is not null) { yield return Object; }
            if (Property is not null) { yield return Property; }
        }

        /// <inheritdoc />
        public override void ReplaceNodes(NodeReplacer searcher)
        {
            if (searcher(Object, out var outObj))
            {
                Object = outObj;
                outObj?.ReplaceNodes(searcher);
            }

            if (searcher(Property, out var outProp))
            {
                Property = outProp;
                outProp?.ReplaceNodes(searcher);
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
        public bool Equals(MemberNode node)
        {
            return Computed == node.Computed &&
                   Equals(Object, node.Object) &&
                   Equals(Property, node.Property);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Computed, Object, Property);
        }
    }
}
