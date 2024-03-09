using JsepSharp.Plugins.SyntaxTree;
using JsepSharp.SyntaxTree;

namespace JsepSharp.Plugins.Tests
{
    [TestClass]
    public class ObjectTests : AstBuilderWithPlugins
    {
        private static SyntaxNode Parse(string expr)
        {
            return Jsep.Parse(expr, new[] { typeof(TernaryPlugin), typeof(ObjectPlugin) });
        }

        [TestMethod]
        public void TestBasicObjectExpression()
        {
            var expr = @"({ a: 1, b: 2 })";

            var actual = Parse(expr);
            var expected = Obj(
                Prop(false, Id("a"), Lit(1, "1"), false),
                Prop(false, Id("b"), Lit(2, "2"), false));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestVariableKey()
        {
            var expr = @"{ [key || key2]: { a: 0 } }";

            var actual = Parse(expr);
            var expected = Obj(
                Prop(true,
                    Bin("||", Id("key"), Id("key2")),
                    Obj(
                        Prop(false, Id("a"), Lit(0, "0"), false)),
                    false));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMemberExpression()
        {
            var expr = @"{a:1}[b]";

            var actual = Parse(expr);
            var expected = Member(true,
                Obj(
                    Prop(false, Id("a"), Lit(1, "1"), false)),
                Id("b"));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestNestedObjects()
        {
            var expr = @"{ a: { b: { c: 1 } } }";

            var actual = Parse(expr);
            var expected = Obj(
                Prop(false,
                    Id("a"),
                    Obj(
                        Prop(false, Id("b"),
                            Obj(
                                Prop(false, Id("c"), Lit(1, "1"), false)),
                            false)),
                    false));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestNestedComplexTernaryWithObjectPlugin()
        {
            var expr = @"a ? b ? 1 : c ? d ? e ? 3 : 4 : 5 : 6 : 7";

            var actual = Parse(expr);
            var expected = IIf(
                Id("a"),
                IIf(
                    Id("b"),
                    Lit(1, "1"),
                    IIf(
                        Id("c"),
                        IIf(
                            Id("d"),
                            IIf(
                                Id("e"),
                                Lit(3, "3"),
                                Lit(4, "4")),
                            Lit(5, "5")),
                        Lit(6, "6"))),
                Lit(7, "7"));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestNestedConsequentAlternateTernaryWithObjectPlugin()
        {
            var expr = @"a ? 0 : b ? 1 : 2";

            var actual = Parse(expr);
            var expected = IIf(
                Id("a"),
                Lit(0, "0"),
                IIf(
                    Id("b"),
                    Lit(1, "1"),
                    Lit(2, "2")));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestNestedTernaryWithObjectValues()
        {
            var expr = @"a ? { a: 1 } : b ? { b: 1 } : { c: 1 }[c] === 1 ? 'c' : null";

            var actual = Parse(expr);
            var expected = IIf(
                Id("a"), 
                Obj(
                    Prop(false, Id("a"), Lit(1, "1"), false)), 
                IIf(
                    Id("b"), 
                    Obj(
                        Prop(false, Id("b"), Lit(1, "1"), false)),
                    IIf(
                        Bin("===",
                            Member(true,
                                Obj(
                                    Prop(false, Id("c"), Lit(1, "1"), false)),
                                Id("c")),
                            Lit(1, "1")),
                        Lit("c", "'c'"),
                        Lit(null, "null"))));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParserNoErrors()
        {
            var expressions = new string[] {
                "{ a: b ? 1 : 2, c }",  // mixed object/ternary
                "fn({ a: 1 })",         // function argument
                "a ? 0 : b ? 1 : 2"     // nested ternary with no ()
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
                () => Parse("{ a: }"),
                "missing value");
            Assert.AreEqual("Unexpected object property at character 5.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("{ a: 1"),
                "missing }");
            Assert.AreEqual("Missing } at character 6.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("{ a: 2 ? 3, b }"),
                "missing : in ternary");
            Assert.AreEqual("Expected : at character 10.", ex.Message);
        }
    }
}
