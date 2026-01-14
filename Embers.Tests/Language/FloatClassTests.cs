using Embers.Language;

namespace Embers.Tests.Language
{
    [TestClass]
    public class FloatClassTests
    {
        private FloatClass @class;

        [TestInitialize]
        public void Setup() => @class = new FloatClass(null);

        [TestMethod]
        public void FloatClassInstance()
        {
            Assert.IsNotNull(@class);
            Assert.AreEqual("Float", @class.Name);
            Assert.AreEqual("Float", @class.ToString());
        }

        [TestMethod]
        public void GetClassInstanceMethod() => Assert.IsNotNull(@class.GetInstanceMethod("class"));

        [TestMethod]
        public void GetUnknownInstanceMethod() => Assert.IsNull(@class.GetInstanceMethod("foo"));
    }
}
