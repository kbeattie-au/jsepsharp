using JsepSharp.Plugins.SyntaxTree;
using JsepSharp.SyntaxTree;

namespace JsepSharp.Plugins.Tests
{
    [TestClass]
    public class ArrowTests : AstBuilderWithPlugins
    {
        private static SyntaxNode Parse(string expr)
        {
            return Jsep.Parse(expr, new[] { typeof(ArrowPlugin) });
        }

        [TestMethod]
        public void TestNoArguments()
        {
            var expr = @"a.find(() => true)";

            var actual = Parse(expr);
            var expected = Call(
                Member(false, Id("a"), Id("find")),
                [
                    Arrow(Lit(true, "true"))
                ]);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSingleNonParenthesizedArgument()
        {
            var expr = @"[1, 2].find(v => v === 2)";

            var actual = Parse(expr);
            var expected = Call(
                Member(false, Ar(Lit(1, "1"), Lit(2, "2")), Id("find")),
                [
                    Arrow([Id("v")], Bin("===", Id("v"), Lit(2, "2")))
                ]
            );

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSingleParenthesizedArgument()
        {
            var expr = @"[1, 2].find((v) => v === 2)";

            var actual = Parse(expr);
            var expected = Call(
                Member(false, Ar(Lit(1, "1"), Lit(2, "2")), Id("find")),
                [
                    Arrow([Id("v")], Bin("===", Id("v"), Lit(2, "2")))
                ]
            );

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMultipleParenthesizedArguments()
        {
            var expr = @"a.find((val, key) => key === ""abc"")";

            var actual = Parse(expr);
            var expected = Call(
                Member(false, Id("a"), Id("find")),
                [
                    Arrow([Id("val"), Id("key")], Bin("===", Id("key"), Lit("abc", "\"abc\"")))
                ]);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestNested()
        {
            var expr = @"x => y => a + b";

            var actual = Parse(expr);
            var expected = Arrow(
                [Id("x")], 
                Arrow([Id("y")],
                    Bin("+", Id("a"), Id("b"))));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParserNoErrors()
        {
			var expressions = new[]
            {
                @"([""a"", ""b""].find(v => v === ""b"").length > 1 || 2) === true",
                @"a.find(val => key === ""abc"")",
                @"a.find(() => []).length > 2",
                @"(a || b).find(v => v(1))",
                @"a.find((  ) => 1)"
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
                () => Parse("() => "),
                "detects missing expression");
            Assert.AreEqual("Expected expression after => at character 6.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("a.find((  ) => )"),
                "detects missing expression with parenthesis");
            Assert.AreEqual("Expected expression after => at character 15.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("a.find((   "),
                "detects arrow not started");
            Assert.AreEqual("Unclosed ( at character 11.", ex.Message);
        }
    }
}