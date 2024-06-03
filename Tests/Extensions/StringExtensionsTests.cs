using JsepSharp.Extensions;

namespace JsepSharp.Tests.Extensions
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void TestCharAt()
        {
            Assert.AreEqual("a".CharAt(0), 'a');
            Assert.AreEqual("ab".CharAt(0), 'a');
            Assert.AreEqual("abc".CharAt(1), 'b');
            Assert.AreEqual("abc".CharAt(2), 'c');
            Assert.AreEqual("".CharAt(0), Jsep.NO_MORE);
            Assert.AreEqual("".CharAt(1), Jsep.NO_MORE);
            Assert.AreEqual(StringExtensions.CharAt(null, 1), Jsep.NO_MORE);
        }

        [TestMethod]
        public void TestFromTo()
        {
            Assert.AreEqual("ab".FromTo(0, 0), "");
            Assert.AreEqual("ab".FromTo(0, 1), "a");
            Assert.AreEqual("ab".FromTo(0, 2), "ab");
            Assert.AreEqual("abc".FromTo(1, 2), "b");
            Assert.AreEqual("abc".FromTo(1, 3), "bc");
            Assert.AreEqual("".FromTo(1, 3), "");
            Assert.AreEqual(StringExtensions.FromTo(null, 0, 1), "");
        }

        [TestMethod]
        public void TestSubstringSafeLengthOnly()
        {
            Assert.AreEqual("abcdef".SubstringSafe(0), "abcdef");
            Assert.AreEqual("abcdef".SubstringSafe(1), "bcdef");
            Assert.AreEqual("abcdef".SubstringSafe(40), "");
            Assert.AreEqual(StringExtensions.SubstringSafe(null, 0), "");
        }

        [TestMethod]
        public void TestSubstringSafeLengthAndIndex()
        {
            Assert.AreEqual("abcdef".SubstringSafe(0, 3), "abc");
            Assert.AreEqual("abcdef".SubstringSafe(1, 2), "bc");
            Assert.AreEqual("abcdef".SubstringSafe(0, 40), "abcdef");
            Assert.AreEqual("abcdef".SubstringSafe(40, 0), "");
            Assert.AreEqual(StringExtensions.SubstringSafe(null, 0, 4), "");
        }
    }
}