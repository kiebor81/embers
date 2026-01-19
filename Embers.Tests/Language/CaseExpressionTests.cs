using Embers.Exceptions;

namespace Embers.Tests.Language
{
    [TestClass]
    public class CaseExpressionTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup() => machine = new Machine();

        private object Execute(string code) => machine.ExecuteText(code);

        [TestMethod]
        public void CaseStatement_MatchesWhenWithSubject()
        {
            var result = Execute("x = 1\ncase x\nwhen 1 then 'one'\nelse 'other'\nend");
            Assert.AreEqual("one", result);
        }

        [TestMethod]
        public void CaseStatement_UsesElseWhenNoMatch()
        {
            var result = Execute("x = 2\ncase x\nwhen 1 then 'one'\nelse 'other'\nend");
            Assert.AreEqual("other", result);
        }

        [TestMethod]
        public void CaseStatement_WithoutSubjectUsesTruthyWhen()
        {
            var result = Execute("x = 5\ncase\nwhen x > 3 then 'yes'\nwhen x > 10 then 'no'\nend");
            Assert.AreEqual("yes", result);
        }

        [TestMethod]
        public void CaseStatement_MatchesRangePattern()
        {
            var result = Execute("case 5\nwhen 1..10 then 'hit'\nelse 'miss'\nend");
            Assert.AreEqual("hit", result);
        }

        [TestMethod]
        public void CaseStatement_MatchesRegexPatternWithToString()
        {
            var result = Execute("case 123\nwhen /23/ then 'yes'\nelse 'no'\nend");
            Assert.AreEqual("yes", result);
        }

        [TestMethod]
        public void CaseStatement_MatchesProcPattern()
        {
            var result = Execute("is_even = ->(x) { x % 2 == 0 }\ncase 4\nwhen is_even then 'even'\nelse 'odd'\nend");
            Assert.AreEqual("even", result);
        }

        [TestMethod]
        public void CaseStatement_MatchesMultiPatternWhen()
        {
            var result = Execute("case 70\nwhen 60, 70 then 'ok'\nelse 'no'\nend");
            Assert.AreEqual("ok", result);
        }

        [TestMethod]
        public void CaseStatement_MatchesClassPattern()
        {
            var result = Execute("case 'name'\nwhen String then 'string'\nelse 'other'\nend");
            Assert.AreEqual("string", result);
        }

        [TestMethod]
        public void CaseStatement_UsesCustomTripleEquals()
        {
            var result = Execute("class Even\n  def ===(obj)\n    (obj % 2) == 0\n  end\nend\ncase 4\nwhen Even.new then 'even'\nelse 'odd'\nend");
            Assert.AreEqual("even", result);
        }

        [TestMethod]
        public void CaseIn_BindsNestedHashValue()
        {
            var result = Execute("config = {db: {user: 'admin', password: 'x'}}\ncase config\nin db: {user:} then user\nelse 'none'\nend");
            Assert.AreEqual("admin", result);
        }

        [TestMethod]
        public void CaseIn_DoesNotOverwriteBindingsOnMismatch()
        {
            var result = Execute("user = 'prev'\nconfig = {db: {password: 'x'}}\ncase config\nin db: {user:} then 'hit'\nelse 'miss'\nend\nuser");
            Assert.AreEqual("prev", result);
        }

        [TestMethod]
        public void CaseIn_ExpressionPatternMatches()
        {
            var result = Execute("x = 2\ncase x\nin 1 then 'one'\nin 2 then 'two'\nelse 'other'\nend");
            Assert.AreEqual("two", result);
        }

        [TestMethod]
        public void CaseIn_HashPatternMismatchFallsThrough()
        {
            var result = Execute("case 5\nin db: {user:} then 'hit'\nelse 'miss'\nend");
            Assert.AreEqual("miss", result);
        }

        [TestMethod]
        public void TripleEquals_IsInvalidOutsideCase()
        {
            var ex = Assert.ThrowsException<InvalidOperationError>(() => Execute("1 === 1"));
            Assert.AreEqual("=== is only supported in case matching", ex.Message);
        }
    }
}
