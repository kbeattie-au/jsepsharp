using JsepNet.Plugins.SyntaxTree;
using JsepNet.SyntaxTree;

namespace JsepNet.Plugins.Tests
{
    [TestClass]
    public class AssignmentTests : AstBuilderWithPlugins
    {
        private static SyntaxNode Parse(string expr)
        {
            return Jsep.Parse(expr, new[] { typeof(TernaryPlugin), typeof(AssignmentPlugin) });
        }

        [TestMethod]
        public void TestBasicParsesAssignments()
        {
            string[] operators =
            [
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

            var left = Id("a");
            var right = Lit(2, "2");

            foreach (var op in operators)
            {
                var expr = $"a {op} 2";
                var actual = Parse(expr);
                var expected = Assign(op, left, right);

                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestParsesChainedAssignments()
        {
            var expr = @"a = b = c = d";

            var actual = Parse(expr);
            var expected = Assign("=",
                Id("a"),
                Assign("=",
                    Id("b"),
                    Assign("=",
                        Id("c"),
                        Id("d"))));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParsesChainedAssignmentsWithTernary()
        {
            var expr = @"a = b = c ? d : e = 2";

            var actual = Parse(expr);
            var expected = Assign("=",
                Id("a"),
                Assign("=",
                    Id("b"),
                    IIf(
                        Id("c"),
                        Id("d"),
                        Assign("=", Id("e"), Lit(2, "2")))));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParsesAssignmentInTernaryAlternate()
        {
            var expr = @"a ? fn(a) : a = 1";

            var actual = Parse(expr);
            var expected = IIf(
                Id("a"),
                Call(Id("fn"), [Id("a")]),
                Assign("=", Id("a"), Lit(1, "1")));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParsesAssignmentInTernaryConsequentAndAlternate()
        {
            var expr = @"a = b + 2 ? c = 3 : d = 4";

            var actual = Parse(expr);
            var expected = Assign("=",
                Id("a"),
                IIf(
                    Bin("+",
                        Id("b"),
                        Lit(2, "2")),
                    Assign("=", Id("c"), Lit(3, "3")),
                    Assign("=", Id("d"), Lit(4, "4"))));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestBasicParsesUpdates()
        {
            string[] operators =
            [
                "++",
                "--"
            ];

            var arg = Id("a");

            foreach (var op in operators)
            {
                var expr = $"{op}a";
                var actual = Parse(expr);
                var expected = Update(op, arg, true);
                Assert.AreEqual(expected, actual);

                expr = $"a{op}";
                actual = Parse(expr);
                expected = Update(op, arg, false);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void TestParserErrors()
        {
            Exception ex;

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("fn()++"),
                "cannot postfix update function result");
            Assert.AreEqual("Unexpected at character 4.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("1++"),
                "cannot postfix update numeric literal");
            Assert.AreEqual("Missing unaryOp argument at character 3.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("++"),
                "variable required for increment");
            Assert.AreEqual("No identifier at character 2.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("(a + b)++"),
                "cannot postfix update binary operator result");
            Assert.AreEqual("Unexpected + at character 7.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("--fn()"),
                "cannot prefix update function result");
            Assert.AreEqual("Unexpected -- at character 6.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("--1"),
                "cannot prefix update numeric literal");
            Assert.AreEqual("Unexpected 1 at character 2.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("--"),
                "variable required for decrement");
            Assert.AreEqual("No identifier at character 2.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("--(a + b)"),
                "cannot prefix update binary operator result");
            Assert.AreEqual("Unexpected ( at character 2.", ex.Message);
        }
    }
}
