using JsepNet.Plugins.SyntaxTree;
using JsepNet.SyntaxTree;

namespace JsepNet.Plugins.Tests
{
    [TestClass]
    public class CommentTests : AstBuilderWithPlugins
    {
        private static SyntaxNode Parse(string expr)
        {
            return Jsep.Parse(expr, new[] { typeof(CommentPlugin) });
        }

        [TestMethod]
        public void TestCommentsOnly()
        {
            var expected = Comp();

            var expr = "/**/";
            var actual = Parse(expr);
            Assert.AreEqual(expected, actual);

            expr = "/* Ignore */";
            actual = Parse(expr);
            Assert.AreEqual(expected, actual);

            expr = "//";
            actual = Parse(expr);
            Assert.AreEqual(expected, actual);

            expr = "// Ignore me.";
            actual = Parse(expr);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestCommentsIgnored()
        {
            var expressions = new string[]
            {
                "a /* ignore this */> 1 // ignore this too",
                "a/* ignore this */    > 1 // ignore this too",
                "(a/* ignore this */  )>/**/ 1 // ignore this too",
                "a /* ignore *\r\n *this */> 1 // ignore this too",
                "a /*ignore *\r\n *this *///\n> 1 // ignore this too",
                "a // ignore this\r\n> 1",
                "a /** {param} \r\n */> 1",
                "// a\na > 1",
                "// a\n   //\n    a > 1",
            };

            var expected = Bin(">", Id("a"), Lit(1, "1"));

            foreach (var expr in expressions)
            {
                var actual = Parse(expr);
                Assert.AreEqual(expected, actual, $"Expression: \"{expr}\"");
            }
        }

        [TestMethod]
        public void TestCommentUnclosed()
        {
            var ex = Assert.ThrowsException<ParsingException>(
                () => Parse("a /* no close comment"),
                "unclosed comment");
            Assert.AreEqual("Missing closing comment, */ at character 22.", ex.Message);
        }
    }
}
