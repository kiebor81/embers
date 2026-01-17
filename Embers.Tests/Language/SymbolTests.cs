using Embers.Language.Primitive;

namespace Embers.Tests.Language
{
    [TestClass]
    public class SymbolTests
    {
        [TestMethod]
        public void CreateSymbol()
        {
            Symbol symbol = new("foo");

            Assert.AreEqual(":foo", symbol.ToString());
        }

        [TestMethod]
        public void Equals()
        {
            Symbol symbol1 = new("foo");
            Symbol symbol2 = new("bar");
            Symbol symbol3 = new("foo");

            Assert.IsTrue(symbol1.Equals(symbol3));
            Assert.IsTrue(symbol3.Equals(symbol1));
            Assert.AreEqual(symbol1.GetHashCode(), symbol3.GetHashCode());

            Assert.IsFalse(symbol1.Equals(symbol2));
            Assert.IsFalse(symbol2.Equals(symbol1));
            Assert.IsFalse(symbol1.Equals(null));
            Assert.IsFalse(symbol1.Equals("foo"));
        }
    }
}
