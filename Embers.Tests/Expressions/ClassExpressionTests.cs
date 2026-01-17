using Embers.Expressions;
using Embers.StdLib;

namespace Embers.Tests.Expressions
{
    [TestClass]
    public class ClassExpressionTests
    {
        [TestMethod]
        public void DefineSimpleClass()
        {
            Machine machine = new();
            Context context = machine.RootContext;
            StringWriter writer = new();
            context.Self.Class.SetInstanceMethod("puts", new PutsFunction(writer));
            ClassExpression expr = new(new NameExpression("Dog"), new CallExpression("puts", [new ConstantExpression(123)]));

            var result = expr.Evaluate(context);

            Assert.IsNull(result);

            var value = context.GetValue("Dog");
            Assert.IsInstanceOfType(value, typeof(DynamicClass));
            Assert.AreEqual(value, context.GetValue("Dog"));
            Assert.AreEqual("123\r\n", writer.ToString());
        }

        [TestMethod]
        public void GetLocalVariables()
        {
            ClassExpression expr = new(new NameExpression("Dog"), new CallExpression("puts", [new ConstantExpression(123)]));
            Assert.IsNull(expr.GetLocalVariables());
        }

        [TestMethod]
        public void DefineSimpleSubClass()
        {
            Machine machine = new();
            Context context = machine.RootContext;
            DynamicClass animalclass = new("Animal", (DynamicClass)context.GetValue("Object"));
            context.SetLocalValue("Animal", animalclass);
            StringWriter writer = new();
            context.Self.Class.SetInstanceMethod("puts", new PutsFunction(writer));
            ClassExpression expr = new(new NameExpression("Dog"), new CallExpression("puts", [new ConstantExpression(123)]), new NameExpression("Animal"));

            var result = expr.Evaluate(context);

            Assert.IsNull(result);

            var value = context.GetValue("Dog");
            Assert.IsInstanceOfType(value, typeof(DynamicClass));
            Assert.AreEqual(value, context.GetValue("Dog"));
            Assert.AreSame(animalclass, ((DynamicClass)value).SuperClass);
            Assert.AreEqual("123\r\n", writer.ToString());
        }

        [TestMethod]
        public void RedefineSimpleClass()
        {
            Machine machine = new();
            Context context = machine.RootContext;
            StringWriter writer = new();
            context.Self.Class.SetInstanceMethod("puts", new PutsFunction(writer));
            ClassExpression expr = new(new NameExpression("Dog"), new CallExpression("puts", [new ConstantExpression(123)]));

            expr.Evaluate(context);

            var initial = context.GetValue("Dog");

            var result = expr.Evaluate(context);

            Assert.IsNull(result);

            var value = context.GetValue("Dog");
            Assert.IsInstanceOfType(value, typeof(DynamicClass));
            Assert.AreEqual(value, context.GetValue("Dog"));
            Assert.AreSame(initial, value);
            Assert.AreEqual("123\r\n123\r\n", writer.ToString());
        }

        [TestMethod]
        public void Equals()
        {
            ClassExpression expr1 = new(new NameExpression("foo"), new ConstantExpression(1));
            ClassExpression expr2 = new(new NameExpression("bar"), new ConstantExpression(1));
            ClassExpression expr3 = new(new NameExpression("foo"), new ConstantExpression(2));
            ClassExpression expr4 = new(new NameExpression("foo"), new ConstantExpression(1));

            Assert.IsTrue(expr1.Equals(expr4));
            Assert.IsTrue(expr4.Equals(expr1));
            Assert.AreEqual(expr1.GetHashCode(), expr4.GetHashCode());

            Assert.IsFalse(expr1.Equals(null));
            Assert.IsFalse(expr1.Equals(123));

            Assert.IsFalse(expr1.Equals(expr2));
            Assert.IsFalse(expr2.Equals(expr1));
            Assert.IsFalse(expr1.Equals(expr3));
            Assert.IsFalse(expr3.Equals(expr1));
        }
    }
}
