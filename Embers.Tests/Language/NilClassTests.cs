namespace Embers.Tests.Language
{
    [TestClass]
    public class NilClassTests
    {
        private NilClass @class;

        [TestInitialize]
        public void Setup() => @class = new NilClass(null);

        [TestMethod]
        public void NilClassName()
        {
            Assert.AreEqual("NilClass", @class.Name);
            Assert.AreEqual("NilClass", @class.ToString());
        }

        [TestMethod]
        public void GetClassInstanceMethod() => Assert.IsNotNull(@class.GetInstanceMethod("class"));

        [TestMethod]
        public void GetUnknownInstanceMethod() => Assert.IsNull(@class.GetInstanceMethod("foo"));
    }
}
