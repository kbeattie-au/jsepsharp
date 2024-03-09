using JsepSharp.Extensions;

namespace JsepSharp.Plugins
{
    /// <summary>
    /// Treats all comments as whitespace to remove from parsing.
    /// </summary>
    public sealed class CommentPlugin : Plugin
    {
        /// <inheritdoc />
        public override string Name => "Comment";

        public CommentPlugin(Jsep parser) : base(parser)
        {
            parser.AfterSpaces += Parser_AfterSpaces;
        }

        // Port: 'gobble-spaces' hook.
        void Parser_AfterSpaces()
        {
            if (Parser.CharCode == Jsep.FSLASH_CODE)
            {
                var ch = Parser.Expression.CharAt(Parser.Index + 1);
                if (ch == Jsep.FSLASH_CODE)
                {
                    // '//': read to end of line/input.
                    Parser.Index++;

                    while (ch != Jsep.LF_CODE && ch != Jsep.NO_MORE)
                    {
                        ++Parser.Index;
                        ch = Parser.CharCode;
                    }

                    Parser.GobbleSpaces();
                }
                else if (ch == Jsep.ASTSK_CODE)
                {
                    // Read to */ or end of input.
                    Parser.Index += 2;

                    while (ch != Jsep.NO_MORE)
                    {
                        ch = Parser.CharCode;
                        Parser.Index++;

                        if (ch == Jsep.ASTSK_CODE)
                        {
                            ch = Parser.CharCode;
                            Parser.Index++;

                            if (ch == Jsep.FSLASH_CODE)
                            {
                                Parser.GobbleSpaces();
                                return;
                            }
                        }
                    }

                    throw Parser.Error("Missing closing comment, */");
                }
            }
        }
    }
}
