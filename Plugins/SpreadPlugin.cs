using JsepNet.Plugins.SyntaxTree;
using JsepNet.Extensions;

namespace JsepNet.Plugins
{
    /// <summary>
    /// Adds rudimentary spread operator support (...).
    /// </summary>
    public sealed class SpreadPlugin : Plugin
    {
        static readonly int[] indices = [0, 1, 2];

        /// <inheritdoc />
        public override string Name => "Spread";

        /// <inheritdoc />
        public SpreadPlugin(Jsep parser) : base(parser)
        {
            parser.BeforeToken += Parser_BeforeToken;
        }

        // Port: 'gobble-token' hook.
        void Parser_BeforeToken(NodeEvent env)
        {
            // Works in objects { ...a }, arrays [...a], function args fn(...a)
            // NOTE: does not prevent `a ? ...b : ...c` or `...123`.
            if (indices.All(i => Parser.Expression.CharAt(Parser.Index + i) == Jsep.PERIOD_CODE))
            {
                Parser.Index += 3;
                env.Node = new SpreadNode(Parser.GobbleExpression());
            }
        }
    }
}
