using Embers.Language.Primitive;

namespace Embers.Tests.Language
{
    [TestClass]
    public class StringClassTests
    {
        private StringClass @class;

        [TestInitialize]
        public void Setup() => @class = new StringClass(null);

        [TestMethod]
        public void StringClassInstance()
        {
            Assert.IsNotNull(@class);
            Assert.AreEqual("String", @class.Name);
        }

        [TestMethod]
        public void GetClassInstanceMethod() => Assert.IsNotNull(@class.GetInstanceMethod("class"));

        [TestMethod]
        public void GetUnknownInstanceMethod() => Assert.IsNull(@class.GetInstanceMethod("foo"));
    }
}
