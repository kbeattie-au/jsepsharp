using JsepNet.Plugins.SyntaxTree;
using JsepNet.SyntaxTree;

namespace JsepNet.Plugins.Tests
{
    [TestClass]
    public class NewTests : AstBuilderWithPlugins
    {
        private static SyntaxNode Parse(string expr)
        {
            return Jsep.Parse(expr, new[] { typeof(NewPlugin), typeof(TernaryPlugin) });
        }

        [TestMethod]
        public void TestBasicNew()
        {
            var expr = @"new Date(123)";

            var actual = Parse(expr);
            var expected = New(Id("Date"), [Lit(123, "123")]);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestBasicNewNoArguments()
        {
            var expr = @"new Date()";

            var actual = Parse(expr);
            var expected = New(Id("Date"));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMemberExpression()
        {
            var expr = @"new Date(123).month";

            var actual = Parse(expr);
            var expected = Member(false, 
                New(Id("Date"), [Lit(123, "123")]),
                Id("month"));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMemberExpressionNested()
        {
            var expr = @"new Date(new Date().setDate(new Date().getDate() - 5))";

            var actual = Parse(expr);
            var expected = New(
                Id("Date"), 
                [
                    Call(
                        Member(false, 
                            New(Id("Date")),
                            Id("setDate")),
                        [
                            Bin("-",
                                Call(
                                    Member(false,
                                        New(Id("Date")),
                                        Id("getDate")), []),
                                Lit(5, "5"))
                        ])
                ]);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParserNoErrors()
        {
            var expressions = new[]
            {
                "new A().b",
                "new A()[\"b\"].c + 2",
                "new A() + 2",
                "new A() != null",
                "new A(), new B()",
                "[new A(), new A()]",
                "new A(\"1\")",
                "new A(1, 2)"
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
                () => Parse("new A"),
                "must be call, not identifier");
            Assert.AreEqual("Expected new function() at character 5.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("new A,new B"),
                "compound not calls");
            Assert.AreEqual("Expected new function() at character 5.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("new A + 2"),
                "must be call, not identifier (binary operator)");
            Assert.AreEqual("Expected new function() at character 6.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("fn(new A)"),
                "must be call, not identifier (nested)");
            Assert.AreEqual("Expected new function() at character 8.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("!new A"),
                "must be call, not identifer (negation)");
            Assert.AreEqual("Expected new function() at character 6.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("new 123"),
                "must be call, not number literal");
            Assert.AreEqual("Expected new function() at character 7.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("new (a > 2 ? A : B)"),
                "must be call, not ternary identifier expression");
            Assert.AreEqual("Expected new function() at character 19.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("new (a > 1)"),
                "must be call, not boolean expression");
            Assert.AreEqual("Expected new function() at character 11.", ex.Message);
        }
    }
}
