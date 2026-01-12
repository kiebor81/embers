using Microsoft.VisualStudio.TestTools.UnitTesting;
using Embers.Language;

namespace Embers.Tests.Language
{
    [TestClass]
    public class BlockParameterTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup()
        {
            machine = new Machine();
        }

        [TestMethod]
        public void TestExplicitBlockParameter()
        {
            machine.Execute("def foo(&block); block.call(5); end");
            var result = machine.Execute("foo { |x| x * 2 }");
            Assert.AreEqual(10L, result);
        }

        [TestMethod]
        public void TestBlockParameterIsProc()
        {
            machine.Execute("def foo(&block); block; end");
            var result = machine.Execute("foo { |x| x * 2 }");
            Assert.IsNotNull(result, "Block parameter should not be null");
            Assert.IsInstanceOfType(result, typeof(Proc), $"Block parameter should be Proc but was {result.GetType().Name}");
        }

        [TestMethod]
        public void TestBlockParameterWithRegularParams()
        {
            machine.Execute("def bar(a, b, &block); block.call(a + b); end");
            var result = machine.Execute("bar(3, 4) { |x| x * 2 }");
            Assert.AreEqual(14L, result);
        }

        [TestMethod]
        public void TestBlockParameterPassedToAnotherMethod()
        {
            machine.Execute("def helper(&block); block.call(10); end");
            machine.Execute("def foo(&block); helper(&block); end");
            var result = machine.Execute("foo { |x| x + 5 }");
            Assert.AreEqual(15L, result);
        }

        [TestMethod]
        public void TestBlockParameterCanBeStored()
        {
            machine.Execute("def foo(&block); @stored = block; end");
            machine.Execute("foo { |x| x * 3 }");
            var result = machine.Execute("@stored.call(7)");
            Assert.AreEqual(21L, result);
        }

        [TestMethod]
        public void TestBlockParameterWithBracketSyntax()
        {
            machine.Execute("def foo(&block); block[5]; end");
            var result = machine.Execute("foo { |x| x * 2 }");
            Assert.AreEqual(10L, result);
        }

        [TestMethod]
        public void TestBlockParameterMultipleCalls()
        {
            machine.Execute("def foo(&block); block.call(1) + block.call(2); end");
            var result = machine.Execute("foo { |x| x * 10 }");
            Assert.AreEqual(30L, result);
        }

        [TestMethod]
        public void TestBlockParameterInClass()
        {
            machine.Execute(@"
                class MyClass
                    def process(&block)
                        block.call(5)
                    end
                end
            ");
            machine.Execute("obj = MyClass.new");
            var result = machine.Execute("obj.process { |x| x + 10 }");
            Assert.AreEqual(15L, result);
        }
    }
}
