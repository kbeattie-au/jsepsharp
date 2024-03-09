using JsepSharp.Plugins.SyntaxTree;
using JsepSharp.SyntaxTree;

namespace JsepSharp.Plugins
{
    /// <summary>
    /// Adds support for `new` expressions.
    /// </summary>
    public sealed class NewPlugin : Plugin
    {
        /// <inheritdoc />
        public override string Name => "New";

        public NewPlugin(Jsep parser) : base(parser)
        {
            Jsep.AddUnaryOp("new");

            parser.AfterToken += Parser_AfterToken;
        }

        // Port: 'after-token' hook.
        void Parser_AfterToken(NodeEvent env)
        {
            if (env.Node is not UnaryNode node) return;

            if (node.Operator == "new")
            {
                var argNode = node.Argument;
                if (argNode is null || !(argNode is CallNode || argNode is MemberNode))
                {
                    throw Parser.Error("Expected new function()");
                }

                // Drop wrapping UnaryNode.
                env.Node = argNode;

                // Change CallNode to NewNode (could be a nested member, even within a call expr).
                MemberNode? memberNode = null;
                while (argNode is MemberNode || (
                    argNode is CallNode callNode && callNode.Callee is MemberNode))
                {
                    memberNode = (MemberNode)(argNode is CallNode callNodeN ? callNodeN.Callee! : argNode);
                    argNode = memberNode.Object;
                }

                if (argNode is CallNode cn)
                {
                    if (memberNode is null)
                    {
                        env.Node = new NewNode(cn);
                    }
                    else
                    {
                        memberNode.Object = new NewNode(cn);
                    }
                }
            }
        }
    }
}
