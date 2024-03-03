using JsepNet.Extensions;
using JsepNet.Plugins.SyntaxTree;

namespace JsepNet.Plugins
{
    /// <summary>
    /// Adds support for async arrow function (and inner await) parsing.
    /// </summary>
    public sealed class AsyncAwaitPlugin : Plugin
    {
        /// <inheritdoc />
        public override string Name => "AsyncAwait";

        readonly static Type[] dependents = [typeof(ArrowPlugin)];
        public override IEnumerable<Type> DependentPlugins { get => dependents; }

        public AsyncAwaitPlugin(Jsep parser) : base(parser)
        {
            parser.BeforeToken += Parser_BeforeToken;
        }

        // Port: 'gobble-token' hook.
        void Parser_BeforeToken(NodeEvent env)
        {
            if (Parser.CharCode == 'a' && !Jsep.IsIdentifierStart(Parser.Expression.CharAt(Parser.Index + 5)))
            {
                var sub = Parser.Expression.SubstringSafe(Parser.Index + 1, 4);
                if (sub == "wait")
                {
                    // found 'await'
                    Parser.Index += 5;

                    var argument = Parser.GobbleToken() ??
                        throw Parser.Error("Unexpected \"await\"");

                    env.Node = new AwaitNode(argument);
                }
                else if (sub == "sync")
                {
                    // found 'async'
                    Parser.Index += 5;
                    env.Node = Parser.GobbleExpression();

                    if (env.Node is ArrowNode arrowNode)
                    {
                        arrowNode.Async = true;
                    }
                    else
                    {
                        throw Parser.Error("Unexpected \"async\"");
                    }
                }
            }
        }
    }
}
