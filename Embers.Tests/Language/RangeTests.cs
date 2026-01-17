using Range = Embers.Language.Primitive.Range;

namespace Embers.Tests.Language
{
    [TestClass]
    public class RangeTests
    {
        [TestMethod]
        public void EmptyRange()
        {
            int total = 0;
            Range range = new(1, 0);

            foreach (int k in range)
                total += k;

            Assert.AreEqual(0, total);
        }

        [TestMethod]
        public void OneNumberRange()
        {
            int total = 0;
            Range range = new(1, 1);

            foreach (int k in range)
                total += k;

            Assert.AreEqual(1, total);
        }

        [TestMethod]
        public void ThreeNumbersRange()
        {
            int total = 0;
            Range range = new(1, 3);

            foreach (int k in range)
                total += k;

            Assert.AreEqual(6, total);
        }

        [TestMethod]
        public void RangeToString()
        {
            Range range = new(1, 3);

            Assert.AreEqual("1..3", range.ToString());
        }
    }
}
