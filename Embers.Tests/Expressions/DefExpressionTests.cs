using Embers.Expressions;
using Embers.Functions;

namespace Embers.Tests.Expressions
{
    [TestClass]
    public class DefExpressionTests
    {
        [TestMethod]
        public void DefineSimpleFunction()
        {
            Machine machine = new();
            Context context = machine.RootContext;
            DefExpression expr = new(new NameExpression("foo"), [], null, null, new CallExpression("puts", [new ConstantExpression(123)]));

            var result = expr.Evaluate(context);

            Assert.IsNull(result);

            var value = context.Self.Class.GetInstanceMethod("foo");
            Assert.IsNotNull(value);
            Assert.IsInstanceOfType(value, typeof(DefinedFunction));
        }

        [TestMethod]
        public void GetLocalVariables()
        {
            DefExpression expr = new(new NameExpression("foo"), [], null, null, new CallExpression("puts", [new ConstantExpression(123)]));
            Assert.IsNull(expr.GetLocalVariables());
        }

        [TestMethod]
        public void DefineFunction()
        {
            Machine machine = new();
            Context context = machine.RootContext;
            DynamicObject obj = new((DynamicClass)context.GetLocalValue("Object"));
            context.SetLocalValue("a", obj);
            DefExpression expr = new(new DotExpression(new NameExpression("a"), "foo", []), [], null, null, new CallExpression("puts", [new ConstantExpression(123)]));

            var result = expr.Evaluate(context);

            Assert.IsNull(result);

            var value = obj.SingletonClass.GetInstanceMethod("foo");
            Assert.IsNotNull(value);
            Assert.IsInstanceOfType(value, typeof(DefinedFunction));

            Assert.IsNull(obj.Class.GetInstanceMethod("foo"));
        }

        [TestMethod]
        public void Equals()
        {
            DefExpression expr1 = new(new NameExpression("foo"), [], null, null, new ConstantExpression(1));
            DefExpression expr2 = new(new NameExpression("bar"), [], null, null, new ConstantExpression(1));
            DefExpression expr3 = new(new NameExpression("foo"), [], null, null, new ConstantExpression(2));
            DefExpression expr4 = new(new NameExpression("foo"), [], null, null, new ConstantExpression(1));

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

        [TestMethod]
        public void EqualsWithParameters()
        {
            DefExpression expr1 = new(new NameExpression("foo"), ["c"], null, null, new ConstantExpression(1));
            DefExpression expr2 = new(new NameExpression("foo"), ["a"], null, null, new ConstantExpression(1));
            DefExpression expr3 = new(new NameExpression("foo"), ["a", "b"], null, null, new ConstantExpression(1));
            DefExpression expr4 = new(new NameExpression("foo"), ["c"], null, null, new ConstantExpression(1));

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
