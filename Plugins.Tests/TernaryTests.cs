using JsepNet.Plugins.SyntaxTree;
using JsepNet.SyntaxTree;

namespace JsepNet.Plugins.Tests
{
    [TestClass]
    public class TernaryTests : AstBuilderWithPlugins
    {
        private static SyntaxNode Parse(string expr)
        {
            return Jsep.Parse(expr, new[] { typeof(TernaryPlugin) });
        }

        [TestMethod]
        public void TestMinimal()
        {
            var expr = @"a ? b : c";

            var actual = Parse(expr);
            var expected = IIf(Id("a"), Id("b"), Id("c"));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestBinaryOp()
        {
            var expr = @"a||b ? c : d";

            var actual = Parse(expr);
            var expected = IIf(
                Bin("||", Id("a"), Id("b")),
                Id("c"),
                Id("d"));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSecondaryTernaryConsequentSide()
        {
            var expr = @"a ? b ? c : 1 : 2";

            var actual = Parse(expr);
            var expected = IIf(
                Id("a"), 
                IIf(Id("b"), Id("c"), Lit(1, "1")), 
                Lit(2, "2"));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestSecondaryTernaryAlternateSide()
        {
            var expr = @"a ? b : c ? 1 : 2";

            var actual = Parse(expr);
            var expected = IIf(
                Id("a"),
                Id("b"),
                IIf(Id("c"), Lit(1, "1"), Lit(2, "2")));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMixedConsequentAlternateNested3()
        {
            var expr = @"a ? b ? 1 : c ? 5 : 6 : 7";

            var actual = Parse(expr);
            var expected = IIf(
                Id("a"), 
                IIf(
                    Id("b"), 
                    Lit(1, "1"), 
                    IIf(Id("c"), Lit(5, "5"), Lit(6, "6"))), 
                Lit(7, "7"));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMixedConsequentAlternateNested5()
        {
            var expr = @"a ? b ? 1 : c ? d ? e ? 3 : 4 : 5 : 6 : 7";

            var actual = Parse(expr);
            var expected = IIf(
                Id("a"), 
                IIf(
                    Id("b"), 
                    Lit(1, "1"), 
                    IIf(Id("c"),
                        IIf(Id("d"),
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
        public void TestParserErrors()
        {
            Exception ex;

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("a ? b : "),
                "missing value");
            Assert.AreEqual("Expected alternate expression at character 8.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("a ? b"),
                "missing :");
            Assert.AreEqual("Expected : at character 5.", ex.Message);

            ex = Assert.ThrowsException<ParsingException>(
                () => Parse("a : b ?"),
                "backwards");
            Assert.AreEqual("Unexpected ':' at character 2.", ex.Message);
        }
    }
}
