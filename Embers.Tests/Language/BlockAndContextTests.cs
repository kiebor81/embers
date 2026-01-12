using System.Collections;

namespace Embers.Tests.Language
{
    [TestClass]
    public class BlockAndContextTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup()
        {
            machine = new Machine();
        }

        private object Execute(string code)
        {
            return machine.ExecuteText(code);
        }

        #region Basic Yield Tests

        [TestMethod]
        public void YieldWithValue()
        {
            var result = Execute(@"
                def test
                    yield 7
                end
                test { |x| x + 3 }
            ");
            Assert.AreEqual(10L, result);
        }

        [TestMethod]
        public void YieldWithMultipleValues()
        {
            var result = Execute(@"
                def test
                    yield 5, 10
                end
                test { |x, y| x * y }
            ");
            Assert.AreEqual(50L, result);
        }

        [TestMethod]
        public void YieldNoValue()
        {
            var result = Execute(@"
                def test
                    yield
                end
                test { 99 }
            ");
            Assert.AreEqual(99L, result);
        }

        [TestMethod]
        public void MultipleYields()
        {
            var result = Execute(@"
                def test
                    sum = 0
                    yield 1
                    sum = sum + (yield 2)
                    sum = sum + (yield 3)
                    sum
                end
                test { |x| x * 2 }
            ");
            // First yield: 1 * 2 = 2, second: 2 * 2 = 4, third: 3 * 2 = 6
            // sum = 0 + 4 + 6 = 10
            Assert.AreEqual(10L, result);
        }

        #endregion

        #region Array Instance Method Block Tests

        [TestMethod]
        public void ArrayMapWithBlock()
        {
            var result = Execute(@"
                arr = [1, 2, 3]
                arr.map { |x| x * 2 }
            ");
            var list = result as IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(2L, list[0]);
            Assert.AreEqual(4L, list[1]);
            Assert.AreEqual(6L, list[2]);
        }

        [TestMethod]
        public void ArraySelectWithBlock()
        {
            var result = Execute(@"
                arr = [1, 2, 3, 4, 5]
                arr.select { |x| x > 2 }
            ");
            var list = result as IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(3L, list[0]);
            Assert.AreEqual(4L, list[1]);
            Assert.AreEqual(5L, list[2]);
        }

        [TestMethod]
        public void ArrayEachWithBlock()
        {
            var result = Execute(@"
                sum = 0
                arr = [1, 2, 3, 4]
                arr.each { |x| sum = sum + x }
                sum
            ");
            Assert.AreEqual(10L, result);
        }

        [TestMethod]
        public void ChainedArrayMethodsWithBlocks()
        {
            var result = Execute(@"
                arr = [1, 2, 3, 4, 5, 6]
                arr.select { |x| x > 2 }.map { |x| x * 2 }
            ");
            var list = result as IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual(6L, list[0]);
            Assert.AreEqual(8L, list[1]);
            Assert.AreEqual(10L, list[2]);
            Assert.AreEqual(12L, list[3]);
        }

        #endregion

        #region Range Block Tests

        [TestMethod]
        public void RangeEachWithBlock()
        {
            var result = Execute(@"
                sum = 0
                (1..5).each { |x| sum = sum + x }
                sum
            ");
            Assert.AreEqual(15L, result);
        }

        #endregion

        #region Block Return and Control Flow Tests

        [TestMethod]
        public void BlockReturnsValue()
        {
            var result = Execute(@"
                def test
                    result = yield 5
                    result + 10
                end
                test { |x| x * 2 }
            ");
            Assert.AreEqual(20L, result);
        }

        [TestMethod]
        public void BlockWithConditional()
        {
            var result = Execute(@"
                def test
                    yield 5
                end
                test { |x| 
                    if x > 3
                        x * 2
                    else
                        x
                    end
                }
            ");
            Assert.AreEqual(10L, result);
        }

        [TestMethod]
        public void BlockWithLocalVariable()
        {
            var result = Execute(@"
                def test
                    yield 5
                end
                test { |x| 
                    y = x * 2
                    y + 3 
                }
            ");
            Assert.AreEqual(13L, result);
        }

        #endregion

        #region Block Closure Tests

        [TestMethod]
        public void BlockClosureOverInstanceVariable()
        {
            var result = Execute(@"
                class Counter
                    def initialize
                        @count = 0
                    end
                    def increment
                        @count = @count + 1
                        yield @count
                    end
                end
                counter = Counter.new
                counter.increment { |x| x * 2 }
            ");
            Assert.AreEqual(2L, result);
        }

        #endregion

        #region Mixed Native and Custom Method Blocks

        [TestMethod]
        public void CustomMethodCallingArrayMethod()
        {
            var result = Execute(@"
                def double_all(arr)
                    arr.map { |x| x * 2 }
                end
                double_all([1, 2, 3])
            ");
            var list = result as IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(2L, list[0]);
            Assert.AreEqual(4L, list[1]);
            Assert.AreEqual(6L, list[2]);
        }

        [TestMethod]
        public void ArrayMethodWithSimpleBlock()
        {
            var result = Execute(@"
                [1, 2, 3].map { |x| x * x }
            ");
            var list = result as IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1L, list[0]);
            Assert.AreEqual(4L, list[1]);
            Assert.AreEqual(9L, list[2]);
        }

        [TestMethod]
        public void NestedArrayMethodsWithBlocks()
        {
            var result = Execute(@"
                [[1, 2], [3, 4], [5, 6]].map { |arr| arr.map { |x| x * 2 } }
            ");
            var outerList = result as IList;
            Assert.IsNotNull(outerList);
            Assert.AreEqual(3, outerList.Count);
            
            var list1 = outerList[0] as IList;
            Assert.IsNotNull(list1);
            Assert.AreEqual(2L, list1[0]);
            Assert.AreEqual(4L, list1[1]);
            
            var list2 = outerList[1] as IList;
            Assert.IsNotNull(list2);
            Assert.AreEqual(6L, list2[0]);
            Assert.AreEqual(8L, list2[1]);
        }

        #endregion

        #region Block Error Handling Tests

        [TestMethod]
        public void MethodRequiresBlockThrowsErrorWhenMissing()
        {
            try
            {
                Execute(@"
                    [1, 2, 3].map
                ");
                Assert.Fail("Should have thrown ArgumentError");
            }
            catch (Exceptions.ArgumentError ex)
            {
                Assert.IsTrue(ex.Message.Contains("map expects a block"));
            }
        }

        [TestMethod]
        public void YieldWithoutBlockThrowsError()
        {
            try
            {
                Execute(@"
                    def test
                        yield
                    end
                    test
                ");
                Assert.Fail("Should have thrown NameError");
            }
            catch (Exceptions.NameError ex)
            {
                Assert.IsTrue(ex.Message.Contains("no block given"));
            }
        }

        #endregion

        #region Block Scope Tests

        [TestMethod]
        public void BlockHasAccessToOuterScope()
        {
            var result = Execute(@"
                x = 10
                def test
                    yield 5
                end
                test { |y| y + x }
            ");
            Assert.AreEqual(15L, result);
        }

        [TestMethod]
        public void BlockCanModifyOuterScope()
        {
            var result = Execute(@"
                x = 10
                def test
                    yield
                end
                test { x = 20 }
                x
            ");
            Assert.AreEqual(20L, result);
        }

        [TestMethod]
        public void BlockParameterShadowsOuterVariable()
        {
            var result = Execute(@"
                x = 10
                def test
                    yield 5
                end
                result = test { |x| x * 2 }
                [result, x]
            ");
            var list = result as IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(10L, list[0]); // Block result
            Assert.AreEqual(10L, list[1]); // Original x unchanged
        }

        #endregion

        #region Complex Integration Tests

        [TestMethod]
        public void ComplexBlockChainWithMultipleOperations()
        {
            var result = Execute(@"
                result = []
                [1, 2, 3, 4, 5].select { |x| x > 2 }.map { |x| x * 2 }.each { |x| result.push(x) }
                result
            ");
            var list = result as IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(6L, list[0]);
            Assert.AreEqual(8L, list[1]);
            Assert.AreEqual(10L, list[2]);
        }

        [TestMethod]
        public void BlockWithComplexLogic()
        {
            var result = Execute(@"
                def process
                    result = []
                    i = 1
                    while i <= 5
                        value = yield i
                        if value > 5
                            result.push(value)
                        end
                        i = i + 1
                    end
                    result
                end
                process { |x| x * x }
            ");
            var list = result as IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(9L, list[0]);
            Assert.AreEqual(16L, list[1]);
            Assert.AreEqual(25L, list[2]);
        }

        [TestMethod]
        public void YieldInNestedMethod()
        {
            // Test that yield works when methods are called with their own blocks
            var result = Execute(@"
                def double
                    yield * 2
                end
                
                def process
                    x = double { 5 }
                    y = double { 7 }
                    x + y
                end
                
                process
            ");
            Assert.AreEqual(24L, result); // (5 * 2) + (7 * 2) = 10 + 14 = 24
        }

        [TestMethod]
        public void BlocksWorkWithInstanceMethods()
        {
            var result = Execute(@"
                class Calculator
                    def initialize
                        @multiplier = 3
                    end
                    def calculate
                        yield 5, @multiplier
                    end
                end
                calc = Calculator.new
                calc.calculate { |x, m| x * m }
            ");
            Assert.AreEqual(15L, result);
        }

        #endregion
    }
}
