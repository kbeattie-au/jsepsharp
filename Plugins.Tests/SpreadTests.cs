using JsepNet.Plugins.SyntaxTree;
using JsepNet.SyntaxTree;

namespace JsepNet.Plugins.Tests
{
    [TestClass]
    public class SpreadTests : AstBuilderWithPlugins
    {
        private static SyntaxNode Parse(string expr)
        {
            return Jsep.Parse(expr, new[] { typeof(SpreadPlugin), typeof(TernaryPlugin) });
        }

        [TestMethod]
        public void TestArraySpread()
        {
            var expr = @"[...a]";

            var actual = Parse(expr);
            var expected = Ar(Spread(Id("a")));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestFunctionSpread()
        {
            var expr = @"fn(1, ...b)";

            var actual = Parse(expr);
            var expected = Call(
                Id("fn"),
                [Lit(1, "1"), Spread(Id("b"))]);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParserNoErrors()
        {
            var expressions = new string[] {
                "fn(...123)", // Note: Invalid iterator is not checked by jsep (same for [....4] = ... 0.4).
				"fn(...\"abc\")",
                "[1, ...[2, 3]]",
                "[1, ...(a ? b : c)]",
            };

            foreach (string expr in expressions)
            {
                Parse(expr);
            }
        }

        [TestMethod]
        public void TestParserErrors()
        {
            Exception ex;

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("[.....5]"),
                "extra ..");
            Assert.AreEqual("Unexpected period at character 5.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("[..2]"),
                "missing .");
            Assert.AreEqual("Unexpected period at character 2.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("[...3"),
                "missing ]");
            Assert.AreEqual("Expected ] at character 5.", ex.Message);
        }
    }
}
