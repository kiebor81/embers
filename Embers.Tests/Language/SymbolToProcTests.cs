using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Embers.Tests.Language
{
    [TestClass]
    public class SymbolToProcTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup()
        {
            machine = new Machine();
        }

        [TestMethod]
        public void TestSymbolToProc()
        {
            machine.Execute("p = :to_s.to_proc");
            var result = machine.Execute("p.call(5)");
            Assert.AreEqual("5", result);
        }

        [TestMethod]
        public void TestSymbolToProcMultipleCalls()
        {
            machine.Execute("p = :to_s.to_proc");
            var result1 = machine.Execute("p.call(10)");
            var result2 = machine.Execute("p.call(20)");
            Assert.AreEqual("10", result1);
            Assert.AreEqual("20", result2);
        }
    }
}
