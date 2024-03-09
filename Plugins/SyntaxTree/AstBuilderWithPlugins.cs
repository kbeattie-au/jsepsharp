using JsepSharp.SyntaxTree;

namespace JsepSharp.Plugins.SyntaxTree
{
    public class AstBuilderWithPlugins : AstBuilder
    {
        protected static ArrowNode Arrow(List<SyntaxNode?> parms, SyntaxNode? body)
        {
            return new ArrowNode(parms, body);
        }

        protected static ArrowNode Arrow(SyntaxNode? body)
        {
            return new ArrowNode([], body);
        }

        protected static ArrowNode AsyncArrow(List<SyntaxNode?> parms, SyntaxNode? body)
        {
            return new ArrowNode(parms, body, true);
        }

        protected static ArrowNode AsyncArrow(SyntaxNode? body)
        {
            return new ArrowNode([], body, true);
        }

        protected static AwaitNode Await(SyntaxNode? arg)
        {
            return new AwaitNode(arg);
        }

        protected static AssignmentNode Assign(string? op, SyntaxNode? left, SyntaxNode? right)
        {
            return new AssignmentNode(op, left, right);
        }

        protected static ObjectNode Obj(params ObjectProperty?[] props)
        {
            return new ObjectNode([.. props]);
        }

        protected static ObjectProperty Prop(bool computed, SyntaxNode? key, SyntaxNode? value, bool shorthand)
        {
            return new ObjectProperty(computed, key, value, shorthand);
        }

        protected static SpreadNode Spread(SyntaxNode? arg)
        {
            return new SpreadNode(arg);
        }

        protected static TernaryNode IIf(SyntaxNode? test, SyntaxNode? consequent, SyntaxNode? alternative)
        {
            return new TernaryNode(test, consequent, alternative);
        }

        protected static NewNode New(SyntaxNode? callee, List<SyntaxNode?> arguments)
        {
            return new NewNode(callee, arguments);
        }

        protected static NewNode New(SyntaxNode? callee)
        {
            return new NewNode(callee, []);
        }

        protected static UpdateNode Update(string? op, SyntaxNode? arg, bool prefix)
        {
            return new UpdateNode(op, arg, prefix);
        }

        protected static RegexLiteral RegExp(string pattern, string flags)
        {
            return new RegexLiteral(pattern, flags);
        }

        protected static TaggedTemplateNode TagLit(SyntaxNode? tag, TemplateLiteralNode? quasi)
        {
            return new TaggedTemplateNode(tag, quasi);
        }

        protected static TemplateLiteralNode TLit(List<TemplateElement?> quasis, List<SyntaxNode?> exprs)
        {
            return new TemplateLiteralNode(quasis, exprs);
        }

        protected static TemplateElement TElem(string? raw, string? cooked, bool tail)
        {
            return new TemplateElement(raw, cooked, tail);
        }
    }
}
