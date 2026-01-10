namespace Embers.Tests.StdLib.Arrays
{
    /// <summary>
    /// Integration tests for Array methods demonstrating instance method usage patterns.
    /// These tests verify that array methods work correctly as instance methods on Array objects.
    /// </summary>
    [TestClass]
    public class ArrayMethodsIntegrationTests
    {
        // ==================== PUSH METHOD TESTS ====================

        [TestMethod]
        public void Array_PushMethod_AddsElementToEnd()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2, 3]
                arr.push(4)
                arr
            ");
            
            Assert.IsNotNull(result);
            var list = result as System.Collections.IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual(4, list[3]);
        }

        [TestMethod]
        public void Array_PushMethod_ReturnsModifiedArray()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2]
                arr.push(3)
            ");
            
            Assert.IsNotNull(result);
            var list = result as System.Collections.IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
        }

        // ==================== POP METHOD TESTS ====================

        [TestMethod]
        public void Array_PopMethod_RemovesAndReturnsLastElement()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2, 3]
                arr.pop
            ");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void Array_PopMethod_ModifiesOriginalArray()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2, 3]
                arr.pop
                arr
            ");
            
            Assert.IsNotNull(result);
            var list = result as System.Collections.IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        public void Array_PopMethod_EmptyArray_ReturnsNil()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = []
                arr.pop
            ");
            
            Assert.IsNull(result);
        }

        // ==================== SHIFT METHOD TESTS ====================

        [TestMethod]
        public void Array_ShiftMethod_RemovesAndReturnsFirstElement()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2, 3]
                arr.shift
            ");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void Array_ShiftMethod_ModifiesOriginalArray()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2, 3]
                arr.shift
                arr
            ");
            
            Assert.IsNotNull(result);
            var list = result as System.Collections.IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(2, list[0]);
        }

        [TestMethod]
        public void Array_ShiftMethod_EmptyArray_ReturnsNil()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = []
                arr.shift
            ");
            
            Assert.IsNull(result);
        }

        // ==================== UNSHIFT METHOD TESTS ====================

        [TestMethod]
        public void Array_UnshiftMethod_AddsElementToBeginning()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [2, 3]
                arr.unshift(1)
                arr
            ");
            
            Assert.IsNotNull(result);
            var list = result as System.Collections.IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1, list[0]);
        }

        // ==================== FIRST METHOD TESTS ====================

        [TestMethod]
        public void Array_FirstMethod_ReturnsFirstElement()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2, 3]
                arr.first
            ");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void Array_FirstMethod_EmptyArray_ReturnsNil()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = []
                arr.first
            ");
            
            Assert.IsNull(result);
        }

        // ==================== LAST METHOD TESTS ====================

        [TestMethod]
        public void Array_LastMethod_ReturnsLastElement()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2, 3]
                arr.last
            ");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void Array_LastMethod_EmptyArray_ReturnsNil()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = []
                arr.last
            ");
            
            Assert.IsNull(result);
        }

        // ==================== JOIN METHOD TESTS ====================

        [TestMethod]
        public void Array_JoinMethod_DefaultSeparator_JoinsWithEmptyString()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2, 3]
                arr.join
            ");
            
            Assert.IsNotNull(result);
            Assert.AreEqual("123", result);
        }

        [TestMethod]
        public void Array_JoinMethod_CustomSeparator_JoinsWithSeparator()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2, 3]
                arr.join(',')
            ");
            
            Assert.IsNotNull(result);
            Assert.AreEqual("1,2,3", result);
        }

        [TestMethod]
        public void Array_JoinMethod_StringElements()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = ['hello', 'world']
                arr.join(' ')
            ");
            
            Assert.IsNotNull(result);
            Assert.AreEqual("hello world", result);
        }

        // ==================== SORT METHOD TESTS ====================

        [TestMethod]
        public void Array_SortMethod_SortsNumbers()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [3, 1, 2]
                arr.sort
            ");
            
            Assert.IsNotNull(result);
            var list = result as System.Collections.IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(2, list[1]);
            Assert.AreEqual(3, list[2]);
        }

        [TestMethod]
        public void Array_SortMethod_SortsStrings()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = ['charlie', 'alice', 'bob']
                arr.sort
            ");
            
            Assert.IsNotNull(result);
            var list = result as System.Collections.IList;
            Assert.IsNotNull(list);
            Assert.AreEqual("alice", list[0]);
            Assert.AreEqual("bob", list[1]);
            Assert.AreEqual("charlie", list[2]);
        }

        // ==================== UNIQ METHOD TESTS ====================

        [TestMethod]
        public void Array_UniqMethod_RemovesDuplicates()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2, 2, 3, 1]
                arr.uniq
            ");
            
            Assert.IsNotNull(result);
            var list = result as System.Collections.IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
        }

        [TestMethod]
        public void Array_UniqMethod_NoDuplicates_ReturnsSameElements()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2, 3]
                arr.uniq
            ");
            
            Assert.IsNotNull(result);
            var list = result as System.Collections.IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
        }

        // ==================== COMPACT METHOD TESTS ====================

        [TestMethod]
        public void Array_CompactMethod_RemovesNilValues()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, nil, 2, nil, 3]
                arr.compact
            ");
            
            Assert.IsNotNull(result);
            var list = result as System.Collections.IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(2, list[1]);
            Assert.AreEqual(3, list[2]);
        }

        [TestMethod]
        public void Array_CompactMethod_NoNils_ReturnsSameElements()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2, 3]
                arr.compact
            ");
            
            Assert.IsNotNull(result);
            var list = result as System.Collections.IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
        }

        // ==================== FLATTEN METHOD TESTS ====================

        [TestMethod]
        public void Array_FlattenMethod_FlattensNestedArrays()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, [2, 3], 4]
                arr.flatten
            ");
            
            Assert.IsNotNull(result);
            var list = result as System.Collections.IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(2, list[1]);
            Assert.AreEqual(3, list[2]);
            Assert.AreEqual(4, list[3]);
        }

        [TestMethod]
        public void Array_FlattenMethod_NoNestedArrays_ReturnsSame()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2, 3]
                arr.flatten
            ");
            
            Assert.IsNotNull(result);
            var list = result as System.Collections.IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
        }

        // ==================== SAMPLE METHOD TESTS ====================

        [TestMethod]
        public void Array_SampleMethod_ReturnsElementFromArray()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2, 3]
                arr.sample
            ");
            
            Assert.IsNotNull(result);
            var list = new List<object> { 1, 2, 3 };
            Assert.IsTrue(list.Contains(result));
        }

        [TestMethod]
        public void Array_SampleMethod_EmptyArray_ReturnsNil()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = []
                arr.sample
            ");
            
            Assert.IsNull(result);
        }

        // ==================== SHUFFLE METHOD TESTS ====================

        [TestMethod]
        public void Array_ShuffleMethod_ReturnsSameElements()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2, 3, 4, 5]
                arr.shuffle
            ");
            
            Assert.IsNotNull(result);
            var list = result as System.Collections.IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(5, list.Count);
            
            // Check all elements are present
            Assert.IsTrue(list.Contains(1));
            Assert.IsTrue(list.Contains(2));
            Assert.IsTrue(list.Contains(3));
            Assert.IsTrue(list.Contains(4));
            Assert.IsTrue(list.Contains(5));
        }

        [TestMethod]
        public void Array_ShuffleMethod_DoesNotModifyOriginal()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2, 3]
                shuffled = arr.shuffle
                arr
            ");
            
            Assert.IsNotNull(result);
            var list = result as System.Collections.IList;
            Assert.IsNotNull(list);
            // Original array should be unchanged
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(2, list[1]);
            Assert.AreEqual(3, list[2]);
        }

        // ==================== MAP METHOD TESTS ====================

        [TestMethod]
        public void Array_MapMethod_TransformsElements()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2, 3]
                arr.map { |x| x * 2 }
            ");
            
            Assert.IsNotNull(result);
            var list = result as System.Collections.IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(2, list[0]);
            Assert.AreEqual(4, list[1]);
            Assert.AreEqual(6, list[2]);
        }

        [TestMethod]
        public void Array_MapMethod_EmptyArray_ReturnsEmptyArray()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = []
                arr.map { |x| x * 2 }
            ");
            
            Assert.IsNotNull(result);
            var list = result as System.Collections.IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(0, list.Count);
        }

        // ==================== SELECT METHOD TESTS ====================

        [TestMethod]
        public void Array_SelectMethod_FiltersElements()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2, 3, 4, 5]
                arr.select { |x| x > 2 }
            ");
            
            Assert.IsNotNull(result);
            var list = result as System.Collections.IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(3, list[0]);
            Assert.AreEqual(4, list[1]);
            Assert.AreEqual(5, list[2]);
        }

        [TestMethod]
        public void Array_SelectMethod_NoMatchingElements_ReturnsEmptyArray()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2, 3]
                arr.select { |x| x > 10 }
            ");
            
            Assert.IsNotNull(result);
            var list = result as System.Collections.IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(0, list.Count);
        }

        // ==================== CHAINING TESTS ====================

        [TestMethod]
        public void Array_MethodChaining_MultipleMethods()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [1, 2, 2, 3, 4, 5]
                arr.uniq.select { |x| x > 2 }.map { |x| x * 2 }
            ");
            
            Assert.IsNotNull(result);
            var list = result as System.Collections.IList;
            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(6, list[0]);  // 3 * 2
            Assert.AreEqual(8, list[1]);  // 4 * 2
            Assert.AreEqual(10, list[2]); // 5 * 2
        }

        [TestMethod]
        public void Array_MethodChaining_WithJoin()
        {
            Machine machine = new();
            var result = machine.ExecuteText(@"
                arr = [3, 1, 2]
                arr.sort.join(', ')
            ");
            
            Assert.IsNotNull(result);
            Assert.AreEqual("1, 2, 3", result);
        }
    }
}
