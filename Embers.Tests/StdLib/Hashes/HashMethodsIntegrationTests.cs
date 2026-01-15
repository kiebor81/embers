using Embers.Language;
using System.Collections;
using System.Linq;

namespace Embers.Tests.StdLib.Hashes
{
    [TestClass]
    public class HashMethodsIntegrationTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup() => machine = new Machine();

        private object Execute(string code) => machine.ExecuteText(code);

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
            Assert.IsTrue(list.Contains(1L));
            Assert.IsTrue(list.Contains(2L));
            Assert.IsTrue(list.Contains(3L));
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
            Assert.AreEqual(3L, result);
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
            Assert.IsTrue(list.Contains(2L));
            Assert.IsTrue(list.Contains(4L));
            Assert.IsTrue(list.Contains(6L));
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
            Assert.AreEqual(3L, hash[new Symbol("c")]);
            Assert.AreEqual(4L, hash[new Symbol("d")]);
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
            Assert.AreEqual(1L, hash[new Symbol("a")]);
            Assert.AreEqual(2L, hash[new Symbol("b")]);
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
            Assert.AreEqual(1L, hash[new Symbol("a")]);
            Assert.AreEqual(3L, hash[new Symbol("b")]);
            Assert.AreEqual(4L, hash[new Symbol("c")]);
        }

        [TestMethod]
        public void HashFetch()
        {
            var result = Execute(@"
                h = {:a => 1, :b => 2}
                h.fetch(:a)
            ");
            Assert.AreEqual(1L, result);
        }

        [TestMethod]
        public void HashFetchWithDefault()
        {
            var result = Execute(@"
                h = {:a => 1}
                h.fetch(:z, 9)
            ");
            Assert.AreEqual(9L, result);
        }

        [TestMethod]
        public void HashHasValue()
        {
            var result1 = Execute(@"
                h = {:a => 1, :b => 2}
                h.has_value?(2)
            ");
            Assert.AreEqual(true, result1);

            var result2 = Execute(@"
                h = {:a => 1, :b => 2}
                h.has_value?(9)
            ");
            Assert.AreEqual(false, result2);
        }

        [TestMethod]
        public void HashDelete()
        {
            var result = Execute(@"
                h = {:a => 1, :b => 2}
                h.delete(:a)
            ");
            Assert.AreEqual(1L, result);
        }

        [TestMethod]
        public void HashMergeBang()
        {
            var result = Execute(@"
                h = {:a => 1}
                h.merge!({:b => 2})
                h
            ");
            var hash = result as IDictionary;
            Assert.IsNotNull(hash);
            Assert.AreEqual(2, hash.Count);
            Assert.AreEqual(1L, hash[new Symbol("a")]);
            Assert.AreEqual(2L, hash[new Symbol("b")]);
        }

        [TestMethod]
        public void HashToA()
        {
            var result = Execute(@"
                h = {:a => 1, :b => 2}
                h.to_a
            ");
            var list = result as IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(2, list.Count);
            Assert.IsTrue(list.Cast<IList>().Any(pair => pair[0].Equals(new Symbol("a")) && pair[1].Equals(1L)));
            Assert.IsTrue(list.Cast<IList>().Any(pair => pair[0].Equals(new Symbol("b")) && pair[1].Equals(2L)));
        }

        [TestMethod]
        public void HashInvert()
        {
            var result = Execute(@"
                h = {:a => 1, :b => 2}
                h.invert
            ");
            var hash = result as IDictionary;
            Assert.IsNotNull(hash);
            Assert.AreEqual(new Symbol("a"), hash[1L]);
            Assert.AreEqual(new Symbol("b"), hash[2L]);
        }

        [TestMethod]
        public void HashEachKey()
        {
            var result = Execute(@"
                h = {:a => 1, :b => 2}
                count = 0
                h.each_key { |k| count = count + 1 }
                count
            ");
            Assert.AreEqual(2L, result);
        }

        [TestMethod]
        public void HashEachValue()
        {
            var result = Execute(@"
                h = {:a => 1, :b => 2}
                sum = 0
                h.each_value { |v| sum = sum + v }
                sum
            ");
            Assert.AreEqual(3L, result);
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
            Assert.IsTrue(list.Contains(4L));
            Assert.IsTrue(list.Contains(6L));
            Assert.IsTrue(list.Contains(8L));
        }
    }
}
