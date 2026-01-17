using System.Reflection;
using Embers.Functions;
using Embers.Tests.Classes;
using Embers.Utilities;

namespace Embers.Tests.Language
{
    [TestClass]
    public class FunctionWrapperTests
    {
        [TestMethod]
        public void InvokeFunctionWrapper()
        {
            FunctionWrapper<int, int, int, Func<int, int, int>> wrapper = new(new Adder(), null);

            var result = wrapper.CreateFunctionDelegate().DynamicInvoke(2, 3);

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void AddEventHandlerToCalculator()
        {
            Listener listener = new();
            Calculator calculator = new();
            FunctionWrapper<int, int, int, Action<int, int>> wrapper = new(listener, null);
            typeof(Calculator).GetEvent("MyEvent").AddEventHandler(calculator, wrapper.CreateActionDelegate());
            calculator.Add(1, 2);

            Assert.AreEqual(1, listener.X);
            Assert.AreEqual(2, listener.Y);

            MethodInfo invoke = typeof(Calculator).GetEvent("MyEvent").EventHandlerType.GetMethod("Invoke");
            MethodInfo add = typeof(Calculator).GetEvent("MyEvent").GetAddMethod();
            Assert.IsNotNull(add);
            Assert.AreEqual(1, add.GetParameters().Count());
            Assert.IsNotNull(invoke);
            var parameters = invoke.GetParameters();
            var returnparameter = invoke.ReturnParameter;
            Assert.IsNotNull(returnparameter);
            Assert.AreEqual("System.Void", returnparameter.ParameterType.FullName);

            Assert.AreEqual(typeof(MyEvent), typeof(Person).GetEvent("NameEvent").EventHandlerType);
        }

        [TestMethod]
        public void AddEventHandlerToPerson()
        {
            NameListener listener = new();
            Person person = new() { FirstName = "Kieran" };
            FunctionWrapper<string, int, MyEvent> wrapper = new(listener, null);
            typeof(Person).GetEvent("NameEvent").AddEventHandler(person, wrapper.CreateFunctionDelegate());
            person.GetName();

            Assert.AreEqual(6, listener.Length);
            Assert.AreEqual("Kieran", listener.Name);
        }

        [TestMethod]
        public void AddFunctionAsCalculatorEventHandler()
        {
            Calculator calculator = new();
            Listener listener = new();
            ObjectUtilities.AddHandler(calculator, "MyEvent", listener, null);
            calculator.Add(1, 2);

            Assert.AreEqual(1, listener.X);
            Assert.AreEqual(2, listener.Y);
        }

        [TestMethod]
        public void AddFunctionAsPersonEventHandler()
        {
            Person person = new() { FirstName = "Kieran" };
            NameListener listener = new();
            ObjectUtilities.AddHandler(person, "NameEvent", listener, null);
            person.GetName();

            Assert.AreEqual(6, listener.Length);
            Assert.AreEqual("Kieran", listener.Name);
        }

        [TestMethod]
        public void AddFunctionWithoutParametersAsPersonEventHandler()
        {
            Person person = new() { FirstName = "Kieran" };
            IntListener listener = new();
            ObjectUtilities.AddHandler(person, "IntEvent", listener, null);
            person.GetName();

            Assert.AreEqual(0, listener.Arity);
        }

        [TestMethod]
        public void CreateThreadStart()
        {
            Context environment = new();
            Runner function = new();
            FunctionWrapper wrapper = new(function, environment);
            Thread th = new(wrapper.CreateThreadStart());
            th.Start();
            th.Join();
            Assert.IsTrue(function.WasInvoked);
        }

        [TestMethod]
        public void CreateActionDelegate()
        {
            Context environment = new();
            Runner function = new();
            FunctionWrapper wrapper = new(function, environment);
            var @delegate = wrapper.CreateActionDelegate();
            @delegate.DynamicInvoke();
            Assert.IsTrue(function.WasInvoked);
        }

        [TestMethod]
        public void CreateFunctionDelegate()
        {
            Context environment = new();
            Runner function = new();
            FunctionWrapper wrapper = new(function, environment);
            var @delegate = wrapper.CreateFunctionDelegate();
            @delegate.DynamicInvoke();
            Assert.IsTrue(function.WasInvoked);
        }

        internal class Runner : IFunction
        {
            public bool WasInvoked { get; set; }

            public object? Apply(DynamicObject self, Context context, IList<object> values)
            {
                WasInvoked = true;
                return null;
            }
        }

        internal class Adder : IFunction
        {
            public object Apply(DynamicObject self, Context context, IList<object> values) => (int)values[0] + (int)values[1];
        }

        internal class Listener : IFunction
        {
            public int X { get; set; }

            public int Y { get; set; }

            public object? Apply(DynamicObject self, Context context, IList<object> arguments)
            {
                X = (int)arguments[0];
                Y = (int)arguments[1];
                return null;
            }
        }

        internal class NameListener : IFunction
        {
            public int Length { get; set; }

            public string Name { get; set; }

            public object Apply(DynamicObject obj, Context context, IList<object> arguments)
            {
                Name = (string)arguments[0];
                Length = Name.Length;
                return Length;
            }
        }

        internal class IntListener : IFunction
        {
            public int Arity { get; set; }

            public object Apply(DynamicObject obj, Context context, IList<object> arguments)
            {
                if (arguments == null)
                    Arity = 0;
                else
                    Arity = arguments.Count;

                return Arity;
            }
        }
    }
}
