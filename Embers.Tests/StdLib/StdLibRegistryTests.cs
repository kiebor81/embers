using Embers.Language;
using Embers.StdLib.Arrays;
using Embers.StdLib.Strings;

namespace Embers.Tests.StdLib
{
    [TestClass]
    public class StdLibRegistryTests
    {
        [TestMethod]
        public void DynamicArray_MapMethod_UsesAutomaticDiscovery()
        {
            // Integration test: DynamicArray should automatically discover map method
            var arr = new DynamicArray { 1, 2, 3 };
            
            var mapMethod = arr.GetMethod("map");
            
            Assert.IsNotNull(mapMethod);
            Assert.IsInstanceOfType(mapMethod, typeof(MapFunction));
        }

        [TestMethod]
        public void DynamicArray_SelectMethod_UsesAutomaticDiscovery()
        {
            var arr = new DynamicArray { 1, 2, 3 };
            
            var selectMethod = arr.GetMethod("select");
            
            Assert.IsNotNull(selectMethod);
            Assert.IsInstanceOfType(selectMethod, typeof(SelectFunction));
        }

        [TestMethod]
        public void DynamicArray_CompactMethod_UsesAutomaticDiscovery()
        {
            var arr = new DynamicArray { 1, null, 2, 3 };
            
            var compactMethod = arr.GetMethod("compact");
            
            Assert.IsNotNull(compactMethod);
            Assert.IsInstanceOfType(compactMethod, typeof(CompactFunction));
        }

        [TestMethod]
        public void DynamicArray_FlattenMethod_UsesAutomaticDiscovery()
        {
            var arr = new DynamicArray { 1, 2, 3 };
            
            var flattenMethod = arr.GetMethod("flatten");
            
            Assert.IsNotNull(flattenMethod);
            Assert.IsInstanceOfType(flattenMethod, typeof(FlattenFunction));
        }

        [TestMethod]
        public void DynamicArray_UniqMethod_UsesAutomaticDiscovery()
        {
            var arr = new DynamicArray { 1, 1, 2, 3 };
            
            var uniqMethod = arr.GetMethod("uniq");
            
            Assert.IsNotNull(uniqMethod);
            Assert.IsInstanceOfType(uniqMethod, typeof(UniqFunction));
        }

        [TestMethod]
        public void DynamicArray_SortMethod_UsesAutomaticDiscovery()
        {
            var arr = new DynamicArray { 3, 1, 2 };
            
            var sortMethod = arr.GetMethod("sort");
            
            Assert.IsNotNull(sortMethod);
            Assert.IsInstanceOfType(sortMethod, typeof(SortFunction));
        }

        [TestMethod]
        public void DynamicArray_FirstMethod_UsesAutomaticDiscovery()
        {
            var arr = new DynamicArray { 1, 2, 3 };
            
            var firstMethod = arr.GetMethod("first");
            
            Assert.IsNotNull(firstMethod);
            Assert.IsInstanceOfType(firstMethod, typeof(FirstFunction));
        }

        [TestMethod]
        public void DynamicArray_LastMethod_UsesAutomaticDiscovery()
        {
            var arr = new DynamicArray { 1, 2, 3 };
            
            var lastMethod = arr.GetMethod("last");
            
            Assert.IsNotNull(lastMethod);
            Assert.IsInstanceOfType(lastMethod, typeof(LastFunction));
        }

        [TestMethod]
        public void DynamicArray_NonExistentMethod_ReturnsNull()
        {
            var arr = new DynamicArray { 1, 2, 3 };
            
            var method = arr.GetMethod("nonexistent_method");
            
            Assert.IsNull(method);
        }

        [TestMethod]
        public void DynamicArray_MultipleMethodCalls_ReturnNewInstances()
        {
            // Test that multiple calls to GetMethod work correctly
            var arr = new DynamicArray { 1, 2, 3 };
            
            var map1 = arr.GetMethod("map");
            var select1 = arr.GetMethod("select");
            var map2 = arr.GetMethod("map");
            
            Assert.IsNotNull(map1);
            Assert.IsNotNull(select1);
            Assert.IsNotNull(map2);
            
            // Each call should return a new instance
            Assert.AreNotSame(map1, map2);
        }

        [TestMethod]
        public void Machine_InitializesStdLibRegistry()
        {
            // Test that creating a Machine initializes the registry
            var machine = new Machine();
            
            // Verify that we can use array methods after Machine initialization
            var arr = new DynamicArray { 1, 2, 3 };
            var mapMethod = arr.GetMethod("map");
            
            Assert.IsNotNull(mapMethod);
        }
    }
}
