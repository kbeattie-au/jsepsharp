using JsepSharp.Extensions;
using JsepSharp.SyntaxTree;

namespace JsepSharp.Plugins
{
    /// <summary>
    /// Adds support for parsing regular expression literals.
    /// </summary>
    public sealed class RegexPlugin : Plugin
    {
        static readonly string knownFlags = "dgimsuvy";

        /// <inheritdoc />
        public override string Name => "Regex";

        public RegexPlugin(Jsep parser) : base(parser)
        {
            parser.BeforeToken += Parser_BeforeToken;
        }

        // Port: 'gobble-token' hook.
        void Parser_BeforeToken(NodeEvent env)
        {
            var node = GobbleRegexLiteral();
            if (node is null) return;

            env.Node = node;
        }

        SyntaxNode? GobbleRegexLiteral()
        {
            if (Parser.CharCode != Jsep.FSLASH_CODE) return null;

            int patternIndex = ++Parser.Index;
            bool inCharSet = false;

            while (Parser.Index < Parser.Expression.Length)
            {
                if (Parser.CharCode == Jsep.FSLASH_CODE && !inCharSet)
                {
                    var pattern = Parser.Expression.FromTo(patternIndex, Parser.Index);
                    string flags = "";

                    while (++Parser.Index < Parser.Expression.Length)
                    {
                        var ch = Parser.CharCode;
#if NET8_0_OR_GREATER
                        var letterOrDigit = char.IsAsciiLetterOrDigit(ch);
#else
                        var letterOrDigit = Jsep.IsCharAsciiLetterOrDigit(ch);
#endif

                        if (letterOrDigit)
                        {
                            // Cannot have duplicate or unknown flags.
                            // Port: JavaScript version of this plugin does not validate these behaviors.
                            if (!knownFlags.Contains(ch) || flags.Contains(ch))
                            {
                                throw Parser.Error("invalid regular expression flags");
                            }

                            flags += ch;
                        }
                        else
                        {
                            break;
                        }
                    }

                    RegexLiteral rex;
                    try
                    {
                        rex = new RegexLiteral(pattern, flags);
                    }
                    catch (Exception)
                    {
                        throw Parser.Error("could not parse regular expression");
                    }

                    var litNode = new LiteralNode(rex, Parser.Expression.FromTo(patternIndex - 1, Parser.Index));
                    return Parser.GobbleTokenProperty(litNode);
                }

                if (Parser.CharCode == Jsep.OBRACK_CODE)
                {
                    inCharSet = true;
                }
                else if (inCharSet && Parser.CharCode == Jsep.CBRACK_CODE)
                {
                    inCharSet = false;
                }

                Parser.Index += Parser.CharCode == Jsep.BSLASH_CODE ? 2 : 1;
            }

            throw Parser.Error("unclosed regex");
        }
    }
}
