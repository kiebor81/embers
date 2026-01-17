using Embers.Expressions;

namespace Embers.Tests.Expressions
{
    [TestClass]
    public class AssignInstanceVarExpressionTests
    {
        [TestMethod]
        public void AssignValue()
        {
            AssignInstanceVarExpression expr = new("one", new ConstantExpression(1));
            DynamicObject obj = new(null);
            Context context = new(obj, null);

            var result = expr.Evaluate(context);

            Assert.AreEqual(1, result);
            Assert.AreEqual(1, obj.GetValue("one"));
        }

        [TestMethod]
        public void GetLocalVariables()
        {
            AssignInstanceVarExpression expr = new("one", new ConstantExpression(1));
            Assert.IsNull(expr.GetLocalVariables());
        }

        [TestMethod]
        public void GetLocalVariablesFromExpression()
        {
            AssignInstanceVarExpression cmd = new("one", new AssignExpression("a", new ConstantExpression(1)));

            var result = cmd.GetLocalVariables();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("a", result[0]);
        }

        [TestMethod]
        public void Equals()
        {
            AssignInstanceVarExpression expr1 = new("a", new ConstantExpression(1));
            AssignInstanceVarExpression expr2 = new("a", new ConstantExpression(2));
            AssignInstanceVarExpression expr3 = new("b", new ConstantExpression(1));
            AssignInstanceVarExpression expr4 = new("a", new ConstantExpression(1));

            Assert.IsTrue(expr1.Equals(expr4));
            Assert.IsTrue(expr4.Equals(expr1));
            Assert.AreEqual(expr1.GetHashCode(), expr4.GetHashCode());

            Assert.IsFalse(expr1.Equals(null));
            Assert.IsFalse(expr1.Equals(expr2));
            Assert.IsFalse(expr1.Equals(expr3));
            Assert.IsFalse(expr1.Equals(123));
        }
    }
}
