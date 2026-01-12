using Microsoft.VisualStudio.TestTools.UnitTesting;
using Embers.Language;

namespace Embers.Tests.Language
{
    [TestClass]
    public class LambdaTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup()
        {
            machine = new Machine();
        }

        [TestMethod]
        public void TestLambdaCreation()
        {
            var result = machine.Execute("f = lambda { |x| x * 2 }");
            Assert.IsInstanceOfType(result, typeof(Proc));
        }

        [TestMethod]
        public void TestLambdaCall()
        {
            machine.Execute("f = lambda { |x| x * 2 }");
            var result = machine.Execute("f.call(5)");
            Assert.AreEqual(10L, result);
        }

        [TestMethod]
        public void TestLambdaBracketSyntax()
        {
            machine.Execute("f = lambda { |x| x * 2 }");
            var result = machine.Execute("f[5]");
            Assert.AreEqual(10L, result);
        }

        [TestMethod]
        public void TestProcKeyword()
        {
            machine.Execute("f = proc { |x| x * 2 }");
            var result = machine.Execute("f.call(5)");
            Assert.AreEqual(10L, result);
        }

        [TestMethod]
        public void TestLambdaClosure()
        {
            machine.Execute("n = 10");
            machine.Execute("f = lambda { |x| x + n }");
            var result = machine.Execute("f.call(5)");
            Assert.AreEqual(15L, result);
        }

        [TestMethod]
        public void TestLambdaClosurePreservesValue()
        {
            machine.Execute("n = 10");
            machine.Execute("f = lambda { |x| x + n }");
            machine.Execute("n = 20"); // Change n after lambda creation
            var result = machine.Execute("f.call(5)");
            // Lambda captures the binding, not the value - so it sees the updated n
            Assert.AreEqual(25L, result);
        }

        [TestMethod]
        public void TestLambdaNoParameters()
        {
            machine.Execute("f = lambda { 42 }");
            var result = machine.Execute("f.call");
            Assert.AreEqual(42L, result);
        }

        [TestMethod]
        public void TestLambdaMultipleParameters()
        {
            machine.Execute("f = lambda { |x, y| x + y }");
            var result = machine.Execute("f.call(3, 7)");
            Assert.AreEqual(10L, result);
        }

        [TestMethod]
        public void TestLambdaAsArgument()
        {
            machine.Execute(@"
                def apply_twice(f, x)
                    f.call(f.call(x))
                end
            ");
            machine.Execute("increment = lambda { |n| n + 1 }");
            var result = machine.Execute("apply_twice(increment, 5)");
            Assert.AreEqual(7L, result);
        }

        [TestMethod]
        public void TestLambdaReturnValue()
        {
            machine.Execute(@"
                def make_adder(n)
                    lambda { |x| x + n }
                end
            ");
            machine.Execute("add5 = make_adder(5)");
            var result = machine.Execute("add5.call(10)");
            Assert.AreEqual(15L, result);
        }

        [TestMethod]
        public void TestLambdaWithDoEnd()
        {
            machine.Execute(@"
                f = lambda do |x|
                    x * 3
                end
            ");
            var result = machine.Execute("f.call(4)");
            Assert.AreEqual(12L, result);
        }

        [TestMethod]
        public void TestProcWithDoEnd()
        {
            machine.Execute(@"
                f = proc do |x|
                    x * 3
                end
            ");
            var result = machine.Execute("f.call(4)");
            Assert.AreEqual(12L, result);
        }

        [TestMethod]
        public void TestLambdaInArray()
        {
            machine.Execute("funcs = [lambda { |x| x * 2 }, lambda { |x| x + 10 }]");
            var result1 = machine.Execute("funcs[0].call(5)");
            var result2 = machine.Execute("funcs[1].call(5)");
            Assert.AreEqual(10L, result1);
            Assert.AreEqual(15L, result2);
        }

        [TestMethod]
        public void TestLambdaAssignToVariable()
        {
            machine.Execute("f = lambda { |x| x * 2 }");
            machine.Execute("g = f");
            var result = machine.Execute("g.call(5)");
            Assert.AreEqual(10L, result);
        }

        [TestMethod]
        public void TestStabbyLambdaNoParams()
        {
            machine.Execute("f = -> { 42 }");
            var result = machine.Execute("f.call");
            Assert.AreEqual(42L, result);
        }

        [TestMethod]
        public void TestStabbyLambdaOneParam()
        {
            machine.Execute("f = ->(x) { x * 2 }");
            var result = machine.Execute("f.call(5)");
            Assert.AreEqual(10L, result);
        }

        [TestMethod]
        public void TestStabbyLambdaMultipleParams()
        {
            machine.Execute("f = ->(x, y) { x + y }");
            var result = machine.Execute("f.call(3, 7)");
            Assert.AreEqual(10L, result);
        }

        [TestMethod]
        public void TestStabbyLambdaBracketSyntax()
        {
            machine.Execute("f = ->(x) { x * 3 }");
            var result = machine.Execute("f[4]");
            Assert.AreEqual(12L, result);
        }

        [TestMethod]
        public void TestStabbyLambdaClosure()
        {
            machine.Execute("n = 5");
            machine.Execute("f = ->(x) { x + n }");
            var result = machine.Execute("f.call(3)");
            Assert.AreEqual(8L, result);
        }

        [TestMethod]
        public void TestStabbyLambdaDoEnd()
        {
            machine.Execute("f = ->(x) do x * 2 end");
            var result = machine.Execute("f.call(5)");
            Assert.AreEqual(10L, result);
        }
    }
}
