namespace JsepSharp.SyntaxTree
{
    /// <summary>
    /// AST builder shortcut methods. Can be used to shorten the code required for manually building an AST.
    /// </summary>
    /// <remarks>
    /// Useful for unit tests, but has other potential uses.
    /// </remarks>
    public abstract class AstBuilder
    {
        protected static LiteralNode Lit(object? value, string raw, bool optional = false)
        {
            return new LiteralNode(value, raw, optional);
        }

        protected static IdentifierNode Id(string? name, bool optional = false)
        {
            return new IdentifierNode(name, optional);
        }

        protected static MemberNode Member(bool computed, SyntaxNode? obj, SyntaxNode? property, bool optional = false)
        {
            return new MemberNode(computed, obj, property, optional);
        }

        protected static CallNode Call(SyntaxNode? callee, List<SyntaxNode?> arguments, bool optional = false)
        {
            return new CallNode(callee, arguments, optional);
        }

        protected static CallNode Call(SyntaxNode? callee, bool optional = false)
        {
            return new CallNode(callee, [], optional);
        }

        protected static CompoundNode Comp(params SyntaxNode?[] body)
        {
            return new CompoundNode(new(body));
        }

        protected static SequenceNode Seq(params SyntaxNode?[] body)
        {
            return new SequenceNode(new(body));
        }

        protected static ArrayNode Ar(params SyntaxNode?[] body)
        {
            return new ArrayNode(new(body));
        }

        protected static ThisNode Self(bool optional = false)
        {
            return new ThisNode(optional);
        }

        protected static UnaryNode Un(string? op, SyntaxNode? argument, bool prefix)
        {
            return new UnaryNode(op, argument, prefix);
        }

        protected static BinaryNode Bin(string? op, SyntaxNode? left, SyntaxNode? right)
        {
            return new BinaryNode(op, left, right);
        }
    }
}
