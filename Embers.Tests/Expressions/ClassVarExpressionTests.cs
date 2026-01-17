using Embers.Expressions;

namespace Embers.Tests.Expressions
{
    [TestClass]
    public class ClassVarExpressionTests
    {
        [TestMethod]
        public void EvaluateUndefinedClassVar()
        {
            ClassVarExpression expr = new("foo");
            DynamicClass cls = new(null);
            DynamicObject obj = new(cls);
            Context context = new(obj, null);

            Assert.IsNull(expr.Evaluate(context));
        }

        [TestMethod]
        public void EvaluateDefinedClassVar()
        {
            ClassVarExpression expr = new("one");
            DynamicClass cls = new(null);
            DynamicObject obj = new(cls);
            cls.SetValue("one", 1);
            Context context = new(obj, null);

            Assert.AreEqual(1, expr.Evaluate(context));
        }

        [TestMethod]
        public void EvaluateDefinedClassVar_FromClassContext()
        {
            ClassVarExpression expr = new("one");
            DynamicClass cls = new(null);
            cls.SetValue("one", 1);
            Context context = new(cls, null);

            Assert.AreEqual(1, expr.Evaluate(context));
        }

        [TestMethod]
        public void Equals()
        {
            ClassVarExpression expr1 = new("one");
            ClassVarExpression expr2 = new("two");
            ClassVarExpression expr3 = new("one");

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
