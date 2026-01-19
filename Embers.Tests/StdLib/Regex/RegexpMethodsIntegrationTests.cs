using System.Text.RegularExpressions;

namespace Embers.Tests.StdLib.Regex
{
    [TestClass]
    public class RegexpMethodsIntegrationTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup() => machine = new Machine();

        private object Execute(string code) => machine.ExecuteText(code);

        [TestMethod]
        public void RegexLiteral_CreatesRegexp()
        {
            var result = Execute(@"/\d+/");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Regexp));

            var regexp = (Regexp)result;
            Assert.AreEqual(@"\d+", regexp.Pattern);
        }

        [TestMethod]
        public void RegexLiteral_Options_AreApplied()
        {
            var result = Execute("/abc/i");

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Regexp));

            var regexp = (Regexp)result;
            Assert.IsTrue((regexp.Options & RegexOptions.IgnoreCase) != 0);
        }

        [TestMethod]
        public void StringMatch_AcceptsRegexLiteral()
        {
            var result = Execute(@"'abc123'.match(/\d+/)");

            Assert.IsNotNull(result);
            Assert.AreEqual("123", result);
        }

        [TestMethod]
        public void RegexpMatch_Works()
        {
            var result = Execute(@"/\d+/.match('abc123')");

            Assert.IsNotNull(result);
            Assert.AreEqual("123", result);
        }
    }
}
