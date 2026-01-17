using Embers.Language.Primitive;

namespace Embers.Tests.Language
{
    [TestClass]
    public class FixnumClassTests
    {
        private FixnumClass @class;

        [TestInitialize]
        public void Setup() => @class = new FixnumClass(null);

        [TestMethod]
        public void FixnumClassInstance()
        {
            Assert.IsNotNull(@class);
            Assert.AreEqual("Fixnum", @class.Name);
            Assert.AreEqual("Fixnum", @class.ToString());
        }

        [TestMethod]
        public void GetClassInstanceMethod() => Assert.IsNotNull(@class.GetInstanceMethod("class"));

        [TestMethod]
        public void GetUnknownInstanceMethod() => Assert.IsNull(@class.GetInstanceMethod("foo"));
    }
}
