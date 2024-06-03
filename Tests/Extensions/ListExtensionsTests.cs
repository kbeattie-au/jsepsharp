using JsepSharp.Extensions;
using JsepSharp.SyntaxTree;

namespace JsepSharp.Tests.Extensions
{
    [TestClass]
    public class ListExtensionsTests
    {
        [TestMethod]
        public void TestPush()
        {
            List<string> stack;

            stack = ["a", "b"];
            stack.Push("c");
            CollectionAssert.AreEqual(new string[] { "a", "b", "c" }, stack);

            stack.Clear();
            stack.Push("1");
            CollectionAssert.AreEqual(new string[] { "1" }, stack);
        }

        [TestMethod]
        public void TestPop()
        {
            List<string> stack = ["a", "b", "c"];
            string? popResult;

            popResult = stack.Pop();
            Assert.AreEqual("c", popResult);
            CollectionAssert.AreEqual(new string[] { "a", "b" }, stack);

            popResult = stack.Pop();
            Assert.AreEqual("b", popResult);
            CollectionAssert.AreEqual(new string[] { "a" }, stack);

            popResult = stack.Pop();
            Assert.AreEqual("a", popResult);
            CollectionAssert.AreEqual(Array.Empty<string>(), stack);

            popResult = stack.Pop();
            Assert.AreEqual(null, popResult);
            CollectionAssert.AreEqual(Array.Empty<string>(), stack);
        }

        [TestMethod]
        public void TestPopAndCast()
        {
            var idNode = new IdentifierNode("foo");
            var litNode = new LiteralNode("bar", "'bar'");
            List<SyntaxNode> stack = [litNode, idNode];
            IdentifierNode? popResultId;
            LiteralNode? popResultLit;

            popResultId = stack.PopAndCast<SyntaxNode, IdentifierNode>();
            Assert.AreEqual(idNode, popResultId);
            CollectionAssert.AreEqual(new SyntaxNode[] { litNode }, stack);

            popResultLit = stack.PopAndCast<SyntaxNode, LiteralNode>();
            Assert.AreEqual(litNode, popResultLit);
            CollectionAssert.AreEqual(Array.Empty<SyntaxNode>(), stack);
        }
    }
}
