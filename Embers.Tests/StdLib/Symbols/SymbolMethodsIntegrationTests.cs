namespace Embers.Tests.StdLib.Symbols
{
    [TestClass]
    public class SymbolMethodsIntegrationTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup()
        {
            machine = new Machine();
        }

        [TestMethod]
        public void SymbolToS()
        {
            var result = machine.ExecuteText(":hello.to_s");
            Assert.AreEqual("hello", result);
        }

        [TestMethod]
        public void SymbolLength()
        {
            var result = machine.ExecuteText(":world.length");
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void SymbolSize()
        {
            var result = machine.ExecuteText(":test.size");
            Assert.AreEqual(4, result);
        }

        [TestMethod]
        public void SymbolEmptyTrue()
        {
            var result = machine.ExecuteText(":_empty.empty?");
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void SymbolUpcase()
        {
            var result = machine.ExecuteText(":hello.upcase");
            Assert.IsInstanceOfType(result, typeof(Embers.Language.Symbol));
            Assert.AreEqual("HELLO", ((Embers.Language.Symbol)result).Name);
        }

        [TestMethod]
        public void SymbolDowncase()
        {
            var result = machine.ExecuteText(":WORLD.downcase");
            Assert.IsInstanceOfType(result, typeof(Embers.Language.Symbol));
            Assert.AreEqual("world", ((Embers.Language.Symbol)result).Name);
        }

        [TestMethod]
        public void SymbolCapitalize()
        {
            var result = machine.ExecuteText(":hello_world.capitalize");
            Assert.IsInstanceOfType(result, typeof(Embers.Language.Symbol));
            Assert.AreEqual("Hello_world", ((Embers.Language.Symbol)result).Name);
        }

        [TestMethod]
        public void SymbolCapitalizeMixed()
        {
            var result = machine.ExecuteText(":hELLO.capitalize");
            Assert.IsInstanceOfType(result, typeof(Embers.Language.Symbol));
            Assert.AreEqual("Hello", ((Embers.Language.Symbol)result).Name);
        }

        [TestMethod]
        public void SymbolInspect()
        {
            var result = machine.ExecuteText(":test.inspect");
            Assert.AreEqual(":test", result);
        }

        [TestMethod]
        public void SymbolToSym()
        {
            var result = machine.ExecuteText("sym = :original; sym.to_sym");
            Assert.IsInstanceOfType(result, typeof(Embers.Language.Symbol));
            Assert.AreEqual("original", ((Embers.Language.Symbol)result).Name);
        }

        [TestMethod]
        public void SymbolIntern()
        {
            var result = machine.ExecuteText(":test.intern");
            Assert.IsInstanceOfType(result, typeof(Embers.Language.Symbol));
            Assert.AreEqual("test", ((Embers.Language.Symbol)result).Name);
        }

        [TestMethod]
        public void SymbolChainedOperations()
        {
            var result = machine.ExecuteText(":hello.upcase.to_s");
            Assert.AreEqual("HELLO", result);
        }

        [TestMethod]
        public void SymbolLengthAfterTransform()
        {
            var result = machine.ExecuteText(":hi.upcase.length");
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void SymbolComparisonAfterTransform()
        {
            var result = machine.ExecuteText(":hello.upcase.to_s == :HELLO.to_s");
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void SymbolInspectInInterpolation()
        {
            var result = machine.ExecuteText("\"Symbol: #{:test.inspect}\"");
            Assert.AreEqual("Symbol: :test", result);
        }
    }
}
