using Embers.Expressions;
using Embers.Functions;

namespace Embers.Tests.Language
{
    [TestClass]
    public class DynamicClassTests
    {
        [TestMethod]
        public void CreateDefinedClass()
        {
            DynamicClass dclass = new("Dog");

            Assert.AreEqual("Dog", dclass.Name);
        }

        [TestMethod]
        public void DynamicToString()
        {
            DynamicClass dclass = new("Dog");

            Assert.AreEqual("Dog", dclass.ToString());
        }

        [TestMethod]
        public void UndefinedInstanceMethodIsNull()
        {
            DynamicClass dclass = new("Dog");

            Assert.IsNull(dclass.GetInstanceMethod("foo"));
        }

        [TestMethod]
        public void RetrieveDefinedInstanceMethodIsNull()
        {
            DynamicClass dclass = new("Dog");
            IFunction foo = new DefinedFunction(null, null, null, null, null);
            dclass.SetInstanceMethod("foo", foo);

            Assert.AreSame(foo, dclass.GetInstanceMethod("foo"));
        }

        [TestMethod]
        public void GetEmptyOwnInstanceMethodNames()
        {
            DynamicClass dclass = new("Dog");

            var result = dclass.GetOwnInstanceMethodNames();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetOwnInstanceMethodNames()
        {
            DynamicClass dclass = new("Dog");
            IFunction foo = new DefinedFunction(null, null, null, null, null);
            dclass.SetInstanceMethod("foo", foo);

            var result = dclass.GetOwnInstanceMethodNames();

            Assert.IsNotNull(result);
            Assert.AreNotEqual(0, result.Count);
            Assert.IsTrue(result.Contains("foo"));
        }

        [TestMethod]
        public void CreateInstance()
        {
            DynamicClass dclass = new("Dog");
            IFunction foo = new DefinedFunction(null, null, null, null, null);
            dclass.SetInstanceMethod("foo", foo);

            var result = dclass.CreateInstance();

            Assert.IsNotNull(result);
            Assert.AreSame(dclass, result.Class);
            Assert.AreSame(foo, result.GetMethod("foo"));
        }

        [TestMethod]
        public void ClassHasNewMethod()
        {
            Machine machine = new();
            DynamicClass @class = new((DynamicClass)machine.RootContext.GetLocalValue("Class"), "Dog", (DynamicClass)machine.RootContext.GetLocalValue("Object"));

            var result = @class.GetMethod("new");

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UndefinedMethodIsNull()
        {
            DynamicClass @class = new("Dog");

            Assert.IsNull(@class.GetMethod("foo"));
        }

        [TestMethod]
        public void ApplyNewMethod()
        {
            Machine machine = new();
            DynamicClass @class = new((DynamicClass)machine.RootContext.GetLocalValue("Class"), "Dog", (DynamicClass)machine.RootContext.GetLocalValue("Object"));

            var result = @class.GetMethod("new").Apply(@class, machine.RootContext, null);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicObject));

            var obj = (DynamicObject)result;

            Assert.AreSame(@class, obj.Class);
        }

        [TestMethod]
        public void ApplyNewMethodCallingInitialize()
        {
            Machine machine = new();
            DynamicClass @class = new((DynamicClass)machine.RootContext.GetLocalValue("Class"), "Dog", (DynamicClass)machine.RootContext.GetLocalValue("Object"));
            IFunction initialize = new DefinedFunction(new AssignInstanceVarExpression("age", new ConstantExpression(10)), [], null, null, null);
            @class.SetInstanceMethod("initialize", initialize);

            var result = @class.GetMethod("new").Apply(@class, @class.Constants, []);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DynamicObject));

            var obj = (DynamicObject)result;

            Assert.AreSame(@class, obj.Class);
            Assert.AreEqual(10, obj.GetValue("age"));
        }
    }
}
