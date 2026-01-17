using Embers.Expressions;

namespace Embers.Tests.Expressions
{
    [TestClass]
    public class InstanceVarExpressionTests
    {
        [TestMethod]
        public void EvaluateUndefinedInstanceVar()
        {
            InstanceVarExpression expr = new("foo");
            DynamicObject obj = new(null);
            Context context = new(obj, null);

            Assert.IsNull(expr.Evaluate(context));
        }

        [TestMethod]
        public void EvaluateDefinedInstanceVar()
        {
            InstanceVarExpression expr = new("one");
            DynamicObject obj = new(null);
            obj.SetValue("one", 1);
            Context context = new(obj, null);

            Assert.AreEqual(1, expr.Evaluate(context));
        }

        [TestMethod]
        public void Equals()
        {
            InstanceVarExpression expr1 = new("one");
            InstanceVarExpression expr2 = new("two");
            InstanceVarExpression expr3 = new("one");

            Assert.IsTrue(expr1.Equals(expr3));
            Assert.IsTrue(expr3.Equals(expr1));
            Assert.AreEqual(expr1.GetHashCode(), expr3.GetHashCode());

            Assert.IsFalse(expr1.Equals(null));
            Assert.IsFalse(expr1.Equals(123));
            Assert.IsFalse(expr1.Equals(expr2));
            Assert.IsFalse(expr2.Equals(expr1));
        }
    }
}
