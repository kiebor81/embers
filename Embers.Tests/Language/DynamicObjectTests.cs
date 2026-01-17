using Embers.Functions;

namespace Embers.Tests.Language
{
    [TestClass]
    public class DynamicObjectTests
    {
        private DynamicClass @class;
        private IFunction foo;

        [TestInitialize]
        public void Setup()
        {
            Machine machine = new();
            @class = new DynamicClass((DynamicClass)machine.RootContext.GetLocalValue("Class"), "Dog", (DynamicClass)machine.RootContext.GetLocalValue("Object"));
            foo = new DefinedFunction(null, null, null);
            @class.SetInstanceMethod("foo", foo);
        }

        [TestMethod]
        public void CreateObject()
        {
            DynamicObject obj = new(@class);

            Assert.AreSame(@class, obj.Class);
        }

        [TestMethod]
        public void GetSingletonClass()
        {
            DynamicObject obj = new(@class);

            var singleton = obj.SingletonClass;

            Assert.IsNotNull(singleton);
            Assert.AreSame(obj.Class, singleton.SuperClass);
            Assert.AreEqual(string.Format("#<Class:{0}>", obj.ToString()), singleton.Name);
            Assert.IsNotNull(singleton.Class);
            Assert.AreEqual("Class", singleton.Class.Name);
        }

        [TestMethod]
        public void ObjectToString()
        {
            DynamicObject obj = new(@class);
            var result = obj.ToString();

            Assert.IsTrue(result.StartsWith("#<Dog:0x"));
            Assert.IsTrue(result.EndsWith(">"));
        }

        [TestMethod]
        public void GetUndefinedValue()
        {
            DynamicObject obj = new(@class);

            Assert.IsNull(obj.GetValue("name"));
        }

        [TestMethod]
        public void SetAndGetValue()
        {
            DynamicObject obj = new(@class);

            obj.SetValue("name", "Nero");

            Assert.AreEqual("Nero", obj.GetValue("name"));
        }

        [TestMethod]
        public void GetMethodFromClass()
        {
            DynamicObject obj = new(@class);
            Assert.AreSame(foo, obj.GetMethod("foo"));
        }

        [TestMethod]
        public void GetInitialMethods()
        {
            DynamicObject obj = new(@class);
            Assert.IsNotNull(obj.GetMethod("class"));
            Assert.IsNotNull(obj.GetMethod("methods"));
        }

        [TestMethod]
        public void InvokeClassMethod()
        {
            DynamicObject obj = new(@class);
            Assert.AreSame(@class, obj.GetMethod("class").Apply(obj, null, null));
        }

        [TestMethod]
        public void InvokeMethodsMethod()
        {
            DynamicObject obj = new(@class);
            var result = obj.GetMethod("methods").Apply(obj, null, null);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicArray));

            DynamicArray names = (DynamicArray)result;

            Assert.IsTrue(names.Contains(new Symbol("foo")));
            Assert.IsTrue(names.Contains(new Symbol("class")));
            Assert.IsTrue(names.Contains(new Symbol("methods")));
        }

        [TestMethod]
        public void InvokeSingletonMethodsMethod()
        {
            DynamicObject obj = new(@class);
            var result = obj.GetMethod("singleton_methods").Apply(obj, null, null);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicArray));

            DynamicArray names = (DynamicArray)result;

            Assert.AreEqual(0, names.Count);
        }

        [TestMethod]
        public void GetMethodFromSingletonClass()
        {
            DynamicObject obj = new(@class);
            var newfoo = new DefinedFunction(null, null, null);
            obj.SingletonClass.SetInstanceMethod("foo", newfoo);
            Assert.AreSame(newfoo, obj.GetMethod("foo"));
        }
    }
}
