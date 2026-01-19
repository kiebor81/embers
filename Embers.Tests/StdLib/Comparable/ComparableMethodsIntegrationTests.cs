namespace Embers.Tests.StdLib.Comparable
{
    [TestClass]
    public class ComparableMethodsIntegrationTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup() => machine = new Machine();

        private object Execute(string code) => machine.ExecuteText(code);

        [TestMethod]
        public void Comparable_Between_NumericInside()
        {
            Assert.AreEqual(true, Execute("5.between?(1, 10)"));
        }

        [TestMethod]
        public void Comparable_Between_NumericOutside()
        {
            Assert.AreEqual(false, Execute("5.between?(6, 10)"));
        }

        [TestMethod]
        public void Comparable_Between_StringInside()
        {
            Assert.AreEqual(true, Execute("'b'.between?('a', 'c')"));
        }
    }
}
