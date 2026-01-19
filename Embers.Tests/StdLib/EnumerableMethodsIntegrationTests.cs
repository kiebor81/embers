using System.Collections;

namespace Embers.Tests.StdLib
{
    [TestClass]
    public class EnumerableMethodsIntegrationTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup() => machine = new Machine();

        [TestMethod]
        public void Enumerable_Map_UsesEach()
        {
            var result = machine.ExecuteText(@"
                class Counter
                    include Enumerable
                    def initialize(n)
                        @n = n
                    end
                    def each
                        i = 1
                        while i <= @n
                            yield i
                            i = i + 1
                        end
                    end
                end
                Counter.new(3).map { |x| x * 2 }
            ");

            var list = result as IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(2L, list[0]);
            Assert.AreEqual(4L, list[1]);
            Assert.AreEqual(6L, list[2]);
        }

        [TestMethod]
        public void Enumerable_Select_UsesEach()
        {
            var result = machine.ExecuteText(@"
                class Counter
                    include Enumerable
                    def initialize(n)
                        @n = n
                    end
                    def each
                        i = 1
                        while i <= @n
                            yield i
                            i = i + 1
                        end
                    end
                end
                Counter.new(5).select { |x| x % 2 == 0 }
            ");

            var list = result as IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(2L, list[0]);
            Assert.AreEqual(4L, list[1]);
        }

        [TestMethod]
        public void Enumerable_Reduce_UsesEach()
        {
            var result = machine.ExecuteText(@"
                class Counter
                    include Enumerable
                    def initialize(n)
                        @n = n
                    end
                    def each
                        i = 1
                        while i <= @n
                            yield i
                            i = i + 1
                        end
                    end
                end
                Counter.new(4).reduce { |acc, n| acc + n }
            ");

            Assert.AreEqual(10L, result);
        }

        [TestMethod]
        public void Enumerable_ToA_CollectsMultipleValues()
        {
            var result = machine.ExecuteText(@"
                class Pairer
                    include Enumerable
                    def each
                        yield 1, 2
                        yield 3, 4
                    end
                end
                Pairer.new.to_a
            ");

            var list = result as IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(2, list.Count);

            var first = list[0] as IList;
            var second = list[1] as IList;
            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.AreEqual(1L, first[0]);
            Assert.AreEqual(2L, first[1]);
            Assert.AreEqual(3L, second[0]);
            Assert.AreEqual(4L, second[1]);
        }
    }
}
