using Embers.Language.Primitive;

namespace Embers.Tests.Language
{
    [TestClass]
    public class TrueClassTests
    {
        private TrueClass @class;

        [TestInitialize]
        public void Setup() => @class = new TrueClass(null);

        [TestMethod]
        public void TrueClassInstance()
        {
            Assert.IsNotNull(@class);
            Assert.AreEqual("TrueClass", @class.Name);
        }

        [TestMethod]
        public void GetClassInstanceMethod() => Assert.IsNotNull(@class.GetInstanceMethod("class"));

        [TestMethod]
        public void GetUnknownInstanceMethod() => Assert.IsNull(@class.GetInstanceMethod("foo"));
    }
}
