using JsepNet.Extensions;
using JsepNet.Plugins.SyntaxTree;
using JsepNet.SyntaxTree;

namespace JsepNet.Plugins
{
    /// <summary>
    /// Adds support for assignment expressions. 
    /// Allows assignment (`=`), compound assignment (e.g. `+=`), and increment/decrement (e.g. `++` or `--`, both prefix and postfix).
    /// </summary>
    public sealed class AssignmentPlugin : Plugin
    {
        static readonly string[] assignmentOperators = [
            "=",
            "*=",
            "**=",
            "/=",
            "%=",
            "+=",
            "-=",
            "<<=",
            ">>=",
            ">>>=",
            "&=",
            "^=",
            "|=",
        ];

        /// <inheritdoc />
        public override string Name => "Assignment";

        // Ternary needs to be included before this plugin, or it doesn't work properly with it.
        readonly static Type[] dependents = [typeof(TernaryPlugin)];
        public override IEnumerable<Type> DependentPlugins { get => dependents; }

        /// <inheritdoc />
        public AssignmentPlugin(Jsep parser) : base(parser)
        {
            foreach(var op in assignmentOperators)
            {
                Jsep.AddBinaryOp(op, 0.9, true);
            }

            parser.BeforeToken += Parser_BeforeToken;

            parser.AfterToken += Parser_AfterToken;

            parser.AfterExpression += Parser_AfterExpression;
        }

        // Port: 'gobble-token' hook.
        void Parser_BeforeToken(NodeEvent env)
        {
            var op = Parser.Expression.SubstringSafe(Parser.Index, 2);

            // Handle prefix increment/decrement.
            if (op == "++" || op == "--")
            {
                Parser.Index += 2;
                var arg = Parser.GobbleTokenProperty(Parser.GobbleIdentifier());
                var nodeUpdate = new UpdateNode(op, arg, true);
                env.Node = nodeUpdate;

                if (!(arg is MemberNode || arg is IdentifierNode))
                {
                    throw Parser.Error($"Unexpected {op}");
                }
            }
        }

        // Port: 'after-token' hook.
        void Parser_AfterToken(NodeEvent env)
        {
            if (env.Node is null) return;

            // Handle postfix increment/decrement.
            var node = env.Node;
            var op = Parser.Expression.SubstringSafe(Parser.Index, 2);
            
            if (op == "++" || op == "--")
            {
                if (!(node is MemberNode || node is IdentifierNode))
                {
                    if (node is IHasOperator nodeOp)
                    {
                        throw Parser.Error($"Unexpected {nodeOp.Operator}");
                    }

                    throw Parser.Error($"Unexpected");
                }

                Parser.Index += 2;
                var arg = node;
                env.Node = new UpdateNode(op, arg, false);
            }
        }

        // Port: 'after-expression' hook.
        void Parser_AfterExpression(NodeEvent env)
        {
            if (env.Node is null) return;

            if (UpdateBinariesToAssignments(env.Node, out var nodeOut))
            {
                env.Node = nodeOut;
            }
        }

        /// <summary>
        /// Traverse full tree, converting any sub-object nodes as needed.
        /// </summary>
        /// <param name="node"></param>
        bool UpdateBinariesToAssignments(SyntaxNode? nodeToCheck, out SyntaxNode? nodeOut)
        {
            if (nodeToCheck is BinaryNode bn)
            {
                if (assignmentOperators.Contains(bn.Operator))
                {
                    var nodeAssign = new AssignmentNode(bn.Operator, bn.Left, bn.Right);
                    if (UpdateBinariesToAssignments(nodeAssign.Left, out var outLeft))
                    {
                        nodeAssign.Left = outLeft;
                    }
                    if (UpdateBinariesToAssignments(nodeAssign.Right, out var outRight))
                    {
                        nodeAssign.Right = outRight;
                    }

                    nodeOut = nodeAssign;
                    return true;
                }
            }

            // Port: Rewrote this part from `Object.values(node).forEach` due to C# type system.
            nodeToCheck?.ReplaceNodes(UpdateBinariesToAssignments);

            nodeOut = null;
            return false;
        }
    }
}
