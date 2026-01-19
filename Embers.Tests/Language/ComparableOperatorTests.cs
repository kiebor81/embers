namespace Embers.Tests.Language
{
    [TestClass]
    public class ComparableOperatorTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup() => machine = new Machine();

        private object Execute(string code) => machine.ExecuteText(code);

        [TestMethod]
        public void SpaceshipOperator_ComparesNumbers()
        {
            Assert.AreEqual(-1L, Execute("1 <=> 2"));
            Assert.AreEqual(0L, Execute("2 <=> 2"));
            Assert.AreEqual(1L, Execute("3 <=> 2"));
        }

        [TestMethod]
        public void SpaceshipOperator_ComparesStrings()
        {
            Assert.AreEqual(1L, Execute("'b' <=> 'a'"));
        }

        [TestMethod]
        public void SpaceshipOperator_ReturnsNilForIncompatibleTypes()
        {
            Assert.IsNull(Execute("1 <=> 'a'"));
        }
    }
}
