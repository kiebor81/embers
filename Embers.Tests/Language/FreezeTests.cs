using Embers.Exceptions;

namespace Embers.Tests.Language
{
    [TestClass]
    public class FreezeTests
    {
        [TestMethod]
        public void Array_Frozen_DefaultsFalse()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2]
                arr.frozen?
            ");

            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void Array_Freeze_SetsFrozen()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2]
                arr.freeze
                arr.frozen?
            ");

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void Array_Freeze_BlocksPush()
        {
            Machine machine = new();
            Assert.ThrowsException<FrozenError>(() =>
                machine.ExecuteText(@"
                    arr = [1]
                    arr.freeze
                    arr.push(2)
                "));
        }

        [TestMethod]
        public void Array_Freeze_BlocksIndexedAssignment()
        {
            Machine machine = new();
            Assert.ThrowsException<FrozenError>(() =>
                machine.ExecuteText(@"
                    arr = [1]
                    arr.freeze
                    arr[0] = 2
                "));
        }

        [TestMethod]
        public void Hash_Frozen_DefaultsFalse()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                h = {:a => 1}
                h.frozen?
            ");

            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void Hash_Freeze_SetsFrozen()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                h = {:a => 1}
                h.freeze
                h.frozen?
            ");

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void Hash_Freeze_BlocksDelete()
        {
            Machine machine = new();
            Assert.ThrowsException<FrozenError>(() =>
                machine.ExecuteText(@"
                    h = {:a => 1}
                    h.freeze
                    h.delete(:a)
                "));
        }

        [TestMethod]
        public void Hash_Freeze_BlocksMergeBang()
        {
            Machine machine = new();
            Assert.ThrowsException<FrozenError>(() =>
                machine.ExecuteText(@"
                    h = {:a => 1}
                    h.freeze
                    h.merge!({:b => 2})
                "));
        }

        [TestMethod]
        public void Object_Freeze_BlocksInstanceVariableSet()
        {
            Machine machine = new();
            Assert.ThrowsException<FrozenError>(() =>
                machine.ExecuteText(@"
                    obj = Object.new
                    obj.freeze
                    obj.instance_variable_set(:@foo, 1)
                "));
        }

        [TestMethod]
        public void Object_Freeze_BlocksInstanceVariableAssignment()
        {
            Machine machine = new();
            Assert.ThrowsException<FrozenError>(() =>
                machine.ExecuteText(@"
                    class Thing
                      def set
                        @foo = 1
                      end
                    end
                    obj = Thing.new
                    obj.freeze
                    obj.set
                "));
        }

        [TestMethod]
        public void HostArrayList_Freeze_BlocksPush()
        {
            Machine machine = new();
            Assert.ThrowsException<FrozenError>(() =>
                machine.ExecuteText(@"
                    list = System::Collections::ArrayList.new
                    list.add(1)
                    list.freeze
                    list.push(2)
                "));
        }

        [TestMethod]
        public void HostHashtable_Freeze_BlocksMergeBang()
        {
            Machine machine = new();
            Assert.ThrowsException<FrozenError>(() =>
                machine.ExecuteText(@"
                    dict = System::Collections::Hashtable.new
                    dict.freeze
                    dict.merge!({:a => 1})
                "));
        }

        [TestMethod]
        public void DeepFreeze_FreezesNestedCollections()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                h = {outer: [1, {inner: [2]}]}
                h.deep_freeze
                [h.frozen?, h[:outer].frozen?, h[:outer][1].frozen?, h[:outer][1][:inner].frozen?]
            ");

            Assert.AreEqual("[True, True, True, True]", result.ToString());
        }

        [TestMethod]
        public void DeepFreeze_HandlesCycles()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = []
                arr.push(arr)
                arr.deep_freeze
                arr.frozen?
            ");

            Assert.AreEqual(true, result);
        }
    }
}
