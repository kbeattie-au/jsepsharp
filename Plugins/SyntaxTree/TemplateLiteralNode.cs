using JsepSharp.Extensions;
using JsepSharp.SyntaxTree;
using Newtonsoft.Json;

namespace JsepSharp.Plugins.SyntaxTree
{
    /// <summary>
    /// Represents the interpolation (Expression) and string (Quasi) parts of a template literal.<br />
    /// e.g.
    ///   <c>`1${a}2${c}3`</c>
    /// </summary>
    /// <remarks>
    ///   For processing purposes, evaluate a Quasi followed by an Expression until Quasi.Tail is true.
    ///   Interpolations without characters any other between them will still have empty quasi elements. A literal
    ///   with nothing before the interpolation sequence will still have an empty Quasi entry before it.
    /// </remarks>
    public sealed class TemplateLiteralNode : SyntaxNode
    {
        const string TYPE_NAME = "TemplateLiteral";

        /// <summary>
        /// Node type identifier.
        /// </summary>
        public static readonly int NodeTypeId = Jsep.GetOrRegisterTypeIdFor(typeof(TemplateLiteralNode), TYPE_NAME);

        /// <inheritdoc />
        [JsonIgnore]
        public override int TypeId => NodeTypeId;

        /// <summary>
        /// Initialize a template literal node.
        /// </summary>
        public TemplateLiteralNode() : base()
        {
            Quasis = [];
            Expressions = [];
        }

        /// <summary>
        /// Initialize a template literal node with parameters.
        /// </summary>
        /// <param name="quasis">Nodes representing the constant portions of the literal.</param>
        /// <param name="expressions">Nodes representing the expressions within the literal.</param>
        public TemplateLiteralNode(List<TemplateElement?> quasis, List<SyntaxNode?> expressions) : base()
        {
            Quasis = quasis;
            Expressions = expressions;
        }

        /// <summary>
        /// These represent the string portions of the literal.
        /// </summary>
        public List<TemplateElement?> Quasis { get; set; }

        /// <summary>
        /// These represent the interpolated parts of the literal.
        /// </summary>
        public List<SyntaxNode?> Expressions { get; set; }

        /// <inheritdoc />
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            foreach (var q in Quasis.Compact())
            {
                yield return q;
            }

            foreach (var e in Expressions.Compact())
            {
                yield return e;
            }
        }

        /// <inheritdoc />
        public override void ReplaceNodes(NodeReplacer searcher)
        {
            // Skipping Quasis since they MUST be TemplateElement.
            // There isn't much of a need to replace those nodes anyways.
            for (int i = 0; i < Expressions.Count; i++)
            {
                var node = Expressions[i];
                if (searcher(node, out var nodeOut))
                {
                    Expressions[i] = nodeOut;
                    nodeOut?.ReplaceNodes(searcher);
                }
            }
        }

        /// <inheritdoc />
        public override void ToStringBuilder(NodeStringBuilder sb)
        {
            sb.Start("TLit");
            sb.NodeArray(Quasis);
            sb.Delim();
            sb.NodeArray(Expressions);
            sb.End();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return NodeEquals(this, obj, Equals);
        }

        /// <summary>
        /// Determines if another node of the same type has the same expressions and quasis as this one.
        /// </summary>
        /// <param name="node">The node to compare with the current one.</param>
        /// <returns><c>true</c> if the specified node is equal to the current one; else <c>false</c>.</returns>
        public bool Equals(TemplateLiteralNode node)
        {
            return SequenceEquals(Quasis, node.Quasis) &&
                SequenceEquals(Expressions, node.Expressions);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Quasis, Expressions);
        }
    }
}
