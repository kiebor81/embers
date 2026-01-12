namespace Embers.Tests.StdLib.Ranges
{
    using Embers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RangeMethodsIntegrationTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup()
        {
            machine = new Machine();
        }

        [TestMethod]
        public void RangeToA()
        {
            var result = machine.ExecuteText("(1..5).to_a");
            Assert.IsNotNull(result);
            Assert.AreEqual("[1, 2, 3, 4, 5]", result.ToString());
        }

        [TestMethod]
        public void RangeToAEmpty()
        {
            var result = machine.ExecuteText("(5..4).to_a");
            Assert.IsNotNull(result);
            Assert.AreEqual("[]", result.ToString());
        }

        [TestMethod]
        public void RangeInclude()
        {
            var result = machine.ExecuteText("(1..10).include?(5)");
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void RangeIncludeEdgeStart()
        {
            var result = machine.ExecuteText("(1..10).include?(1)");
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void RangeIncludeEdgeEnd()
        {
            var result = machine.ExecuteText("(1..10).include?(10)");
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void RangeIncludeFalse()
        {
            var result = machine.ExecuteText("(1..10).include?(15)");
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void RangeCover()
        {
            var result = machine.ExecuteText("(1..10).cover?(5)");
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void RangeFirst()
        {
            var result = machine.ExecuteText("(5..10).first");
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void RangeLast()
        {
            var result = machine.ExecuteText("(5..10).last");
            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void RangeSize()
        {
            var result = machine.ExecuteText("(1..10).size");
            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void RangeSizeEmpty()
        {
            var result = machine.ExecuteText("(10..5).size");
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void RangeCount()
        {
            var result = machine.ExecuteText("(1..5).count");
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void RangeMin()
        {
            var result = machine.ExecuteText("(5..15).min");
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void RangeMax()
        {
            var result = machine.ExecuteText("(5..15).max");
            Assert.AreEqual(15, result);
        }

        [TestMethod]
        public void RangeStep()
        {
            var result = machine.ExecuteText(@"
                arr = []
                (1..10).step(2) { |n| arr.push(n) }
                arr
            ");
            Assert.IsNotNull(result);
            Assert.AreEqual("[1, 3, 5, 7, 9]", result.ToString());
        }

        [TestMethod]
        public void RangeStepThree()
        {
            var result = machine.ExecuteText(@"
                arr = []
                (1..10).step(3) { |n| arr.push(n) }
                arr
            ");
            Assert.IsNotNull(result);
            Assert.AreEqual("[1, 4, 7, 10]", result.ToString());
        }

        [TestMethod]
        public void RangeMap()
        {
            var result = machine.ExecuteText("(1..5).map { |n| n * 2 }");
            Assert.IsNotNull(result);
            Assert.AreEqual("[2, 4, 6, 8, 10]", result.ToString());
        }

        [TestMethod]
        public void RangeMapSquare()
        {
            var result = machine.ExecuteText("(1..4).map { |n| n * n }");
            Assert.IsNotNull(result);
            Assert.AreEqual("[1, 4, 9, 16]", result.ToString());
        }

        [TestMethod]
        public void RangeSelect()
        {
            var result = machine.ExecuteText("(1..10).select { |n| n % 2 == 0 }");
            Assert.IsNotNull(result);
            Assert.AreEqual("[2, 4, 6, 8, 10]", result.ToString());
        }

        [TestMethod]
        public void RangeSelectGreaterThanFive()
        {
            var result = machine.ExecuteText("(1..10).select { |n| n > 5 }");
            Assert.IsNotNull(result);
            Assert.AreEqual("[6, 7, 8, 9, 10]", result.ToString());
        }

        [TestMethod]
        public void RangeReject()
        {
            var result = machine.ExecuteText("(1..10).reject { |n| n % 2 == 0 }");
            Assert.IsNotNull(result);
            Assert.AreEqual("[1, 3, 5, 7, 9]", result.ToString());
        }

        [TestMethod]
        public void RangeChainedOperations()
        {
            var result = machine.ExecuteText("(1..10).select { |n| n % 2 == 0 }.map { |n| n * 3 }");
            Assert.IsNotNull(result);
            Assert.AreEqual("[6, 12, 18, 24, 30]", result.ToString());
        }

        [TestMethod]
        public void RangeComplexChain()
        {
            var result = machine.ExecuteText("sum = 0; (1..20).select { |n| n % 3 == 0 }.each { |n| sum = sum + n }; sum");
            Assert.AreEqual(63L, result); // 3+6+9+12+15+18
        }
    }
}
