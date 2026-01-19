using Embers.StdLib.Numeric;

namespace Embers.Tests.StdLib.Numeric
{
    [TestClass]
    public class AbsFunctionTests
    {
        [TestMethod]
        public void Abs_PositiveInt_ReturnsSame()
        {
            var func = new AbsFunction();
            Assert.AreEqual(5L, func.Apply(null, null, [5]));
        }

        [TestMethod]
        public void Abs_NegativeInt_ReturnsPositive()
        {
            var func = new AbsFunction();
            Assert.AreEqual(5L, func.Apply(null, null, [-5]));
        }

        [TestMethod]
        public void Abs_Double_ReturnsPositive()
        {
            var func = new AbsFunction();
            Assert.AreEqual(3.5, func.Apply(null, null, [-3.5]));
        }
    }
}
