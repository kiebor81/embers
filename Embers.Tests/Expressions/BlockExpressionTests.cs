using Embers.Expressions;

namespace Embers.Tests.Expressions
{
    [TestClass]
    public class BlockExpressionTests
    {
        [TestMethod]
        public void EvaluateBlockExpression()
        {
            BlockExpression expr = new(null, new ConstantExpression(1));

            var result = expr.Evaluate(null);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Block));
        }

        [TestMethod]
        public void Equals()
        {
            BlockExpression expr1 = new(["a"], new NameExpression("a"));
            BlockExpression expr2 = new(["a"], new NameExpression("b"));
            BlockExpression expr3 = new(["b"], new NameExpression("a"));
            BlockExpression expr4 = new(["a", "b"], new NameExpression("a"));
            BlockExpression expr5 = new(null, new NameExpression("a"));
            BlockExpression expr6 = new(["a"], new NameExpression("a"));

            Assert.IsFalse(expr1.Equals(123));
            Assert.IsFalse(expr1.Equals(null));
            Assert.IsFalse(expr1.Equals("foo"));

            Assert.IsTrue(expr1.Equals(expr6));
            Assert.IsTrue(expr6.Equals(expr1));
            Assert.AreEqual(expr1.GetHashCode(), expr6.GetHashCode());

            Assert.IsFalse(expr1.Equals(expr2));
            Assert.IsFalse(expr2.Equals(expr1));
            Assert.IsFalse(expr1.Equals(expr3));
            Assert.IsFalse(expr3.Equals(expr1));
            Assert.IsFalse(expr1.Equals(expr4));
            Assert.IsFalse(expr4.Equals(expr1));
            Assert.IsFalse(expr1.Equals(expr5));
            Assert.IsFalse(expr5.Equals(expr1));
        }
    }
}
