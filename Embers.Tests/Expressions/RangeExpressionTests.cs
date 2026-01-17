using Embers.Expressions;
using Range = Embers.Language.Primitive.Range;

namespace Embers.Tests.Expressions
{
    [TestClass]
    public class RangeExpressionTests
    {
        [TestMethod]
        public void SimpleEvaluate()
        {
            RangeExpression expr = new(new ConstantExpression(1), new ConstantExpression(4));

            var result = expr.Evaluate(null);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Range));

            var range = (Range)result;

            int total = 0;

            foreach (int k in range)
                total += k;

            Assert.AreEqual(10, total);
        }

        [TestMethod]
        public void GetLocalVariables()
        {
            RangeExpression expr = new(new ConstantExpression(1), new ConstantExpression(4));
            Assert.IsNull(expr.GetLocalVariables());
        }

        [TestMethod]
        public void GetLocalVariablesWithAssignment()
        {
            RangeExpression expr = new(new AssignExpression("a", new ConstantExpression(1)), new ConstantExpression(4));

            var result = expr.GetLocalVariables();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("a", result[0]);
        }

        [TestMethod]
        public void Equals()
        {
            RangeExpression expr1 = new(new ConstantExpression(1), new ConstantExpression(2));
            RangeExpression expr2 = new(new ConstantExpression(1), new ConstantExpression(3));
            RangeExpression expr3 = new(new ConstantExpression(2), new ConstantExpression(2));
            RangeExpression expr4 = new(new ConstantExpression(1), new ConstantExpression(2));

            Assert.IsFalse(expr1.Equals(null));
            Assert.IsFalse(expr1.Equals(123));
            Assert.IsFalse(expr1.Equals("foo"));

            Assert.IsTrue(expr1.Equals(expr4));
            Assert.IsTrue(expr4.Equals(expr1));
            Assert.AreEqual(expr1.GetHashCode(), expr4.GetHashCode());

            Assert.IsFalse(expr1.Equals(expr2));
            Assert.IsFalse(expr2.Equals(expr1));
            Assert.IsFalse(expr1.Equals(expr3));
            Assert.IsFalse(expr3.Equals(expr1));
        }
    }
}

