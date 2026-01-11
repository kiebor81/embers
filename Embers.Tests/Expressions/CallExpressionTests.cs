using Embers.Expressions;
using Embers.StdLib;

namespace Embers.Tests.Expressions
{
    [TestClass]
    public class CallExpressionTests
    {
        [TestMethod]
        public void CallPutsInteger()
        {
            StringWriter writer = new();
            CallExpression expr = new("puts", [new ConstantExpression(123)]);
            Machine machine = new();
            PutsFunction puts = new(writer);
            machine.RootContext.Self.Class.SetInstanceMethod("puts", puts);

            Assert.IsNull(expr.Evaluate(machine.RootContext));
            Assert.AreEqual("123\r\n", writer.ToString());
        }

        [TestMethod]
        public void GetLocalVariables()
        {
            CallExpression expr = new("puts", [new ConstantExpression(123)]);
            Assert.IsNull(expr.GetLocalVariables());
        }

        [TestMethod]
        public void Equals()
        {
            CallExpression expr1 = new("puts", [new ConstantExpression(1)]);
            CallExpression expr2 = new("put", [new ConstantExpression(1)]);
            CallExpression expr3 = new("puts", [new ConstantExpression(2)]);
            CallExpression expr4 = new("puts", [new ConstantExpression(2), new ConstantExpression(3)]);
            CallExpression expr5 = new("puts", [new ConstantExpression(1)]);

            Assert.IsTrue(expr1.Equals(expr5));
            Assert.IsTrue(expr5.Equals(expr1));
            Assert.AreEqual(expr1.GetHashCode(), expr5.GetHashCode());

            Assert.IsFalse(expr1.Equals(null));
            Assert.IsFalse(expr1.Equals(123));

            Assert.IsFalse(expr1.Equals(expr2));
            Assert.IsFalse(expr2.Equals(expr1));
            Assert.IsFalse(expr1.Equals(expr3));
            Assert.IsFalse(expr3.Equals(expr1));
            Assert.IsFalse(expr1.Equals(expr4));
            Assert.IsFalse(expr4.Equals(expr1));
        }
    }
}
