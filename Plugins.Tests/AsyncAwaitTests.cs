using JsepSharp.Plugins.SyntaxTree;
using JsepSharp.SyntaxTree;

namespace JsepSharp.Plugins.Tests
{
    [TestClass]
    public class AsyncAwaitTests : AstBuilderWithPlugins
    {
        private static SyntaxNode Parse(string expr)
        {
            return Jsep.Parse(expr, new[] { typeof(ArrowPlugin), typeof(AsyncAwaitPlugin) });
        }

        [TestMethod]
        public void TestBasicAwait()
        {
            var expr = @"await a";

            var actual = Parse(expr);
            var expected = Await(Id("a"));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestArrowAsyncAwaitMultiArguments()
        {
            var expr = @"await a.run(async () => await v1)";

            var actual = Parse(expr);
            var expected = Await(
                Call(
                    Member(false, Id("a"), Id("run")),
                    [AsyncArrow(Await(Id("v1")))]));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestArrowAsyncAwaitNoArguments()
        {
            var expr = @"await a.find(async (v1, v2) => await v1(v2))";

            var actual = Parse(expr);
            var expected = Await(
                Call(
                    Member(false, Id("a"), Id("find")),
                    [AsyncArrow(
                        [Id("v1"), Id("v2")],
                        Await(Call(Id("v1"), [Id("v2")])))]));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParseIgnoresLiteralsAndIds()
        {
            var expressions = new[]
            {
                "asyncing(123)",
                "awaiting(123)",
                @"[""async"", ""await""]"
            };

            foreach (var expr in expressions)
            {
                var actual = Parse(expr);
                Assert.IsNotInstanceOfType<ArrowNode>(actual);
            }
        }

        [TestMethod]
        public void TestParserNoErrors()
        {
            var expressions = new[]
            {
                "await a.find(async v => await v)",
                "a.find(async ([v]) => await v)",
                "a.find(async () => await x)"
            };

            foreach (var expr in expressions)
            {
                Parse(expr);
            }
        }

        [TestMethod]
        public void TestParserErrors()
        {
            Exception ex;

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("async 123"),
                "async against number");
            Assert.AreEqual("Unexpected \"async\" at character 9.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("async a + b"),
                "async against expression");
            Assert.AreEqual("Unexpected \"async\" at character 11.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("a.find(async () + 2)"),
                "invalid async use");
            Assert.AreEqual("Unexpected \"async\" at character 19.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("await"),
                "not awaiting anything");
            Assert.AreEqual("Unexpected \"await\" at character 5.", ex.Message);
        }
    }
}