using JsepNet.Plugins.SyntaxTree;
using JsepNet.SyntaxTree;

namespace JsepNet.Plugins.Tests
{
    [TestClass]
    public class RegexTests : AstBuilderWithPlugins
    {
        private static SyntaxNode Parse(string expr)
        {
            return Jsep.Parse(expr, new[] { typeof(TernaryPlugin), typeof(AssignmentPlugin), typeof(RegexPlugin) });
        }

        [TestMethod]
        public void TestBasicPattern()
        {
            var expr = @"/abc/";

            var actual = Parse(expr);
            var expected = Lit(RegExp("abc", ""), expr);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestBasicPatternWithFlags()
        {
            var expr = @"/abc/ig";

            var actual = Parse(expr);
            var expected = Lit(RegExp("abc", "ig"), expr);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestPatternSupportsEscapes()
        {
            var expr = @"/\d{3}/";

            var actual = Parse(expr);
            var expected = Lit(RegExp(@"\d{3}", ""), expr);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestPatternEscapesSlashProperly()
        {
            var expr = @"/^\/$/";

            var actual = Parse(expr);
            var expected = Lit(RegExp(@"^\/$", ""), expr);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestMoreComplexWithinExpression()
        {
            var lit = "/[a-z]{3}/ig";
            var expr = $"a && {lit}.test(b)";

            var actual = Parse(expr);
            var expected = Bin("&&",
                Id("a", true), 
                Call(
                    Member(false,
                        Lit(RegExp("[a-z]{3}", "ig"), lit, true),
                        Id("test", true), true),
                    [Id("b", true)], true));

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParserNoErrors()
        {
            var expressions = new[]
            {
                @"/\d(?=px)/.test(a)",
                @"a / /123/",
                @"/123/ig[""test""](b)",
                @"/123/[""test""](b)",
                @"/\\p{Emoji_Presentation}/gu.test(""ticket to 大阪 costs ¥2000 👌."")",
                @"/abc/+/123/"
            };

            foreach (string expr in expressions)
            {
                Parse(expr);
            }
        }

        [TestMethod]
        public void TestParserErrors()
        {
            var expressions = new[] 
            {
                "/abc",           // unclosed regex
			    "/a/xzw",         // invalid flag
			    "/a/xyz.test(a)", // invalid flag
			    "/a(/",           // unclosed (
			    "/a[/",           // unclosed [
			    "/[\\]/",         // unclosed [
			    "/\\/"            // unclosed regex
            };

            foreach (string expr in expressions)
            {
                Assert.ThrowsException<ParsingException>(
                    () => Parse(expr),
                    $"Expression: {expr}");
            }
        }


        [TestMethod]
        public void TestWithBinaryOperators()
        {
            var lit = @"^\\d+$";
            var expr = $"a /= (/{lit}/.test(b) ? +b / 2 : 1)";

            var actual = Parse(expr);
            var expected = Assign("/=",
                Id("a", true),
                IIf(
                    Call(
                        Member(false,
                            Lit(RegExp(lit, ""), "/^\\\\d+$/", true),
                            Id("test", true),
                            true),
                        [Id("b", true)],
                        true),
                    Bin("/",
                        Un("+", Id("b", true), true),
                        Lit(2, "2", true)),
                    Lit(1, "1", true)));

            Assert.AreEqual(expected, actual);
        }
    }
}
