using JsepSharp.Plugins.SyntaxTree;
using JsepSharp.SyntaxTree;

namespace JsepSharp.Plugins
{
    /// <summary>
    /// Adds parsing support for arrow function expressions:
    ///   <c>() => x</c>,
    ///   <c>v => v,</c>
    ///   <c>(a, b) => v</c>
    /// </summary>
    public class ArrowPlugin : Plugin
    {
        public const string OPERATOR = "=>";
        /// <inheritdoc />
        public override string Name => "Arrow";

        public ArrowPlugin(Jsep parser) : base(parser)
        {
            Jsep.AddBinaryOp(OPERATOR, 0.1, true);

            parser.BeforeExpression += Parser_BeforeExpression;

            parser.AfterExpression += Parser_AfterExpression;
        }

        // Port: 'gobble-expression' hook.
        void Parser_BeforeExpression(NodeEvent env)
        {
            var node = GobbleEmptyArrowArg();
            if (node is null) return;

            env.Node = node;
        }

        // Port: 'after-expression' hook.
        void Parser_AfterExpression(NodeEvent env)
        {
            var node = env.Node;
            if (node is null) return;

            if (UpdateBinariesToArrows(node, out var nodeOut))
            {
                env.Node = nodeOut;
                nodeOut!.ReplaceNodes(UpdateBinariesToArrows);
            }
        }

        /// <summary>
        /// This searches for the special case () => ...
        /// which would normally throw an error because of the invalid LHS to the bin op.
        /// </summary>
        /// <returns>An instance or null if none found.</returns>
        ArrowNode? GobbleEmptyArrowArg()
        {
            Parser.GobbleSpaces();

            if (Parser.CharCode == Jsep.OPAREN_CODE)
            {
                var backupIndex = Parser.Index;

                Parser.Index++;
                Parser.GobbleSpaces();

                if (Parser.CharCode == Jsep.CPAREN_CODE)
                {
                    Parser.Index++;

                    var biop = Parser.GobbleBinaryOp();
                    if (biop == OPERATOR)
                    {
                        // () => ...
                        var body = Parser.GobbleBinaryExpression();

                        return body is null ?
                            throw Parser.Error($"Expected expression after {biop}") :
                            new ArrowNode(body);
                    }
                }

                Parser.Index = backupIndex;
            }

            return null;
        }

        /// <summary>
        /// Traverse full tree, converting any sub-object nodes as needed.
        /// </summary>
        /// <param name="node"></param>
        bool UpdateBinariesToArrows(SyntaxNode? nodeToCheck, out SyntaxNode? nodeOut)
        {
            if (nodeToCheck is BinaryNode bn && bn.Operator == OPERATOR)
            {
                var body = bn.Right;

                if (bn.Left is null)
                {
                    nodeOut = new ArrowNode(body);
                }
                else if (bn.Left is SequenceNode sn)
                {
                    nodeOut = new ArrowNode(sn.Expressions, body);
                }
                else
                {
                    nodeOut = new ArrowNode([bn.Left], body);
                }

                return true;
            }

            nodeOut = null;
            return false;
        }
    }
}
