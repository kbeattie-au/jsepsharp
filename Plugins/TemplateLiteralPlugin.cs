using JsepSharp.Extensions;
using JsepSharp.Plugins.SyntaxTree;
using JsepSharp.SyntaxTree;
using System.Text;

namespace JsepSharp.Plugins
{
    /// <summary>
    /// Adds support for parsing template and tagged template literals.
    /// </summary>
    public sealed class TemplateLiteralPlugin : Plugin
    {
        /// <inheritdoc />
        public override string Name => "TemplateLiteral";

        public TemplateLiteralPlugin(Jsep parser) : base(parser)
        {
            parser.BeforeToken += Parser_BeforeToken;
            parser.AfterToken += Parser_AfterToken;
        }

        // Port: 'gobble-token' hook.
        void Parser_BeforeToken(NodeEvent env)
        {
            var node = GobbleTemplateLiteral();
            if (node is null) return;

            env.Node = node;
        }

        // Port: 'after-token' hook.
        void Parser_AfterToken(NodeEvent env)
        {
            if ((env.Node is IdentifierNode || env.Node is MemberNode) && Parser.CharCode == Jsep.BTICK_CODE)
            {
                env.Node = new TaggedTemplateNode(env.Node, GobbleTemplateLiteral());
            }
        }

        TemplateLiteralNode? GobbleTemplateLiteral()
        {
            if (Parser.CharCode == Jsep.BTICK_CODE)
            {
                TemplateLiteralNode node = new();
                StringBuilder cooked = new();
                StringBuilder raw = new();
                bool closed = false;

                var length = Parser.Expression.Length;

                while (Parser.Index < length)
                {
                    var ch = Parser.Expression.CharAt(++Parser.Index);

                    if (ch == Jsep.BTICK_CODE)
                    {
                        Parser.Index += 1;
                        closed = true;
                        node.Quasis.Add(new TemplateElement(raw.ToString(), cooked.ToString(), closed));

                        return node;
                    }
                    else if (ch == Jsep.DOLLAR_CODE && Parser.Expression.CharAt(Parser.Index + 1) == Jsep.OCURLY_CODE)
                    {
                        Parser.Index += 2;
                        node.Quasis.Add(new TemplateElement(raw.ToString(), cooked.ToString(), closed));

                        raw.Clear();
                        cooked.Clear();

                        var innerExprs = Parser.GobbleExpressions(Jsep.CCURLY_CODE);
                        node.Expressions.AddRange(innerExprs);

                        if (Parser.CharCode != Jsep.CCURLY_CODE)
                        {
                            throw Parser.Error("Unclosed ${");
                        }
                    }
                    else if (ch == Jsep.BSLASH_CODE)
                    {
                        // Check for all of the common escape codes
                        raw.Append(ch);
                        ch = Parser.Expression.CharAt(++Parser.Index);
                        raw.Append(ch);
                        cooked.Append(Jsep.ReplaceEscapeChar(ch));
                    }
                    else
                    {
                        cooked.Append(ch);
                        raw.Append(ch);
                    }
                }

                throw Parser.Error("Unclosed `");
            }

            return null;
        }
    }
}
