namespace Embers.Tests.Language
{
    [TestClass]
    public class DynamicHashTests
    {
        [TestMethod]
        public void EmptyHashToString()
        {
            DynamicHash hash = [];

            Assert.AreEqual("{}", hash.ToString());
        }

        [TestMethod]
        public void HashToString()
        {
            DynamicHash hash = new()
            {
                [new Symbol("one")] = 1,
                [new Symbol("two")] = 2
            };

            var result = hash.ToString();

            Assert.IsTrue(result.StartsWith("{"));
            Assert.IsTrue(result.EndsWith("}"));
            Assert.IsTrue(result.Contains(":one=>1"));
            Assert.IsTrue(result.Contains(":two=>2"));
        }
    }
}
