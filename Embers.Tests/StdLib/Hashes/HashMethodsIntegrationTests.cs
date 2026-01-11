using Embers.Language;
using System.Collections;

namespace Embers.Tests.StdLib.Hashes
{
    [TestClass]
    public class HashMethodsIntegrationTests
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

        [TestMethod]
        public void HashKeys()
        {
            var result = Execute(@"
                h = {:a => 1, :b => 2, :c => 3}
                h.keys
            ");
            var list = result as IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
            Assert.IsTrue(list.Contains(new Symbol("a")));
            Assert.IsTrue(list.Contains(new Symbol("b")));
            Assert.IsTrue(list.Contains(new Symbol("c")));
        }

        [TestMethod]
        public void HashValues()
        {
            var result = Execute(@"
                h = {:a => 1, :b => 2, :c => 3}
                h.values
            ");
            var list = result as IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
            Assert.IsTrue(list.Contains(1));
            Assert.IsTrue(list.Contains(2));
            Assert.IsTrue(list.Contains(3));
        }

        [TestMethod]
        public void HashEach()
        {
            var result = Execute(@"
                h = {:a => 1, :b => 2}
                sum = 0
                h.each { |k, v| sum = sum + v }
                sum
            ");
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void HashMap()
        {
            var result = Execute(@"
                h = {:a => 1, :b => 2, :c => 3}
                h.map { |k, v| v * 2 }
            ");
            var list = result as IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
            Assert.IsTrue(list.Contains(2));
            Assert.IsTrue(list.Contains(4));
            Assert.IsTrue(list.Contains(6));
        }

        [TestMethod]
        public void HashSelect()
        {
            var result = Execute(@"
                h = {:a => 1, :b => 2, :c => 3, :d => 4}
                h.select { |k, v| v > 2 }
            ");
            var hash = result as IDictionary;
            Assert.IsNotNull(hash);
            Assert.AreEqual(2, hash.Count);
            Assert.AreEqual(3, hash[new Symbol("c")]);
            Assert.AreEqual(4, hash[new Symbol("d")]);
        }

        [TestMethod]
        public void HashReject()
        {
            var result = Execute(@"
                h = {:a => 1, :b => 2, :c => 3, :d => 4}
                h.reject { |k, v| v > 2 }
            ");
            var hash = result as IDictionary;
            Assert.IsNotNull(hash);
            Assert.AreEqual(2, hash.Count);
            Assert.AreEqual(1, hash[new Symbol("a")]);
            Assert.AreEqual(2, hash[new Symbol("b")]);
        }

        [TestMethod]
        public void HashHasKey()
        {
            var result1 = Execute(@"
                h = {:a => 1, :b => 2}
                h.has_key?(:a)
            ");
            Assert.AreEqual(true, result1);

            var result2 = Execute(@"
                h = {:a => 1, :b => 2}
                h.has_key?(:z)
            ");
            Assert.AreEqual(false, result2);
        }

        [TestMethod]
        public void HashEmpty()
        {
            var result1 = Execute(@"
                h = {}
                h.empty?
            ");
            Assert.AreEqual(true, result1);

            var result2 = Execute(@"
                h = {:a => 1}
                h.empty?
            ");
            Assert.AreEqual(false, result2);
        }

        [TestMethod]
        public void HashSize()
        {
            var result = Execute(@"
                h = {:a => 1, :b => 2, :c => 3}
                h.size
            ");
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void HashMerge()
        {
            var result = Execute(@"
                h1 = {:a => 1, :b => 2}
                h2 = {:b => 3, :c => 4}
                h1.merge(h2)
            ");
            var hash = result as IDictionary;
            Assert.IsNotNull(hash);
            Assert.AreEqual(3, hash.Count);
            Assert.AreEqual(1, hash[new Symbol("a")]);
            Assert.AreEqual(3, hash[new Symbol("b")]);
            Assert.AreEqual(4, hash[new Symbol("c")]);
        }

        [TestMethod]
        public void HashChainedOperations()
        {
            var result = Execute(@"
                h = {:a => 1, :b => 2, :c => 3, :d => 4}
                h.select { |k, v| v > 1 }.map { |k, v| v * 2 }
            ");
            var list = result as IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
            Assert.IsTrue(list.Contains(4));
            Assert.IsTrue(list.Contains(6));
            Assert.IsTrue(list.Contains(8));
        }
    }
}
