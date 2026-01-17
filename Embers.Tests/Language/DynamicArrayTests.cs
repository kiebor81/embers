namespace Embers.Tests.Language
{
    [TestClass]
    public class DynamicArrayTests
    {
        [TestMethod]
        public void EmptyArrayToString()
        {
            DynamicArray array = [];

            Assert.AreEqual("[]", array.ToString());
        }

        [TestMethod]
        public void ArrayToString()
        {
            DynamicArray array = new()
            {
                [0] = 1,
                [1] = 2
            };

            var result = array.ToString();

            Assert.AreEqual("[1, 2]", result);
        }

        [TestMethod]
        public void ArrayToStringWithNils()
        {
            DynamicArray array = new()
            {
                [0] = 1,
                [3] = 2
            };

            var result = array.ToString();

            Assert.AreEqual("[1, nil, nil, 2]", result);
        }
    }
}
