using Embers.Language.Primitive;

namespace Embers.Tests.Language
{
    [TestClass]
    public class FalseClassTests
    {
        private FalseClass @class;

        [TestInitialize]
        public void Setup() => @class = new FalseClass(null);

        [TestMethod]
        public void FalseClassInstance()
        {
            Assert.IsNotNull(@class);
            Assert.AreEqual("FalseClass", @class.Name);
        }

        [TestMethod]
        public void GetClassInstanceMethod() => Assert.IsNotNull(@class.GetInstanceMethod("class"));

        [TestMethod]
        public void GetUnknownInstanceMethod() => Assert.IsNull(@class.GetInstanceMethod("foo"));
    }
}
