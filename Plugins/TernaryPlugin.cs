using JsepSharp.Plugins.SyntaxTree;
using JsepSharp.SyntaxTree;

namespace JsepSharp.Plugins
{
    /// <summary>
    /// Adds ternary/conditional expression support (`?:`).
    /// </summary>
    public sealed class TernaryPlugin : Plugin
    {
        /// <inheritdoc />
        public override string Name => "Ternary";

        /// <inheritdoc />
        public TernaryPlugin(Jsep parser) : base(parser)
        {
            parser.AfterExpression += Parser_AfterExpression;
        }

        // Port: 'after-expression' hook.
        void Parser_AfterExpression(NodeEvent env)
        {
            var test = env.Node;

            if (test is not null && Parser.CharCode == Jsep.QUMARK_CODE)
            {
                Parser.Index++;

                var consequent = Parser.GobbleExpression() ??
                    throw Parser.Error("Expected consequent expression");

                Parser.GobbleSpaces();

                if (Parser.CharCode == Jsep.COLON_CODE)
                {
                    Parser.Index++;

                    var alternate = Parser.GobbleExpression()
                        ?? throw Parser.Error("Expected alternate expression");

                    var envNode = new TernaryNode(test, consequent, alternate);
                    env.Node = envNode;

                    if (test is BinaryNode testBinary)
                    {
                        // Check for operators of higher priority than ternary (i.e. assignment)
                        // jsep sets || at 1, and assignment at 0.9, and conditional should be between them.
                        if (testBinary.Operator is not null && Jsep.BinaryOps.TryGetValue(testBinary.Operator, out var prec) && prec <= 0.9)
                        {
                            BinaryNode testNew = testBinary;
                            while (testNew.Right is BinaryNode r && r.Operator is not null && Jsep.BinaryPrecedence(r.Operator) <= 0.9)
                            {
                                testNew = (BinaryNode)testNew.Right;
                            }

                            envNode.Test = testNew.Right!;
                            testNew.Right = envNode;

                            env.Node = testBinary;
                        }
                    }
                }
                else
                {
                    throw Parser.Error("Expected :");
                }
            }
        }
    }
}
