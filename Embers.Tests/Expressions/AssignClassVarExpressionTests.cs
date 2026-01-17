using Embers.Expressions;

namespace Embers.Tests.Expressions
{
    [TestClass]
    public class AssignClassVarExpressionTests
    {
        [TestMethod]
        public void AssignValue()
        {
            AssignClassVarExpression cmd = new("one", new ConstantExpression(1));
            DynamicClass cls = new(null);
            DynamicObject obj = new(cls);
            Context context = new(obj, null);

            var result = cmd.Evaluate(context);

            Assert.AreEqual(1, result);
            Assert.AreEqual(1, cls.GetValue("one"));
        }

        [TestMethod]
        public void AssignValue_ClassContext()
        {
            AssignClassVarExpression cmd = new("one", new ConstantExpression(1));
            DynamicClass cls = new(null);
            Context context = new(cls, null);

            var result = cmd.Evaluate(context);

            Assert.AreEqual(1, result);
            Assert.AreEqual(1, cls.GetValue("one"));
        }

        [TestMethod]
        public void GetLocalVariables()
        {
            AssignClassVarExpression cmd = new("one", new ConstantExpression(1));
            Assert.IsNull(cmd.GetLocalVariables());
        }

        [TestMethod]
        public void GetLocalVariablesFromExpression()
        {
            AssignClassVarExpression cmd = new("one", new AssignExpression("a", new ConstantExpression(1)));

            var result = cmd.GetLocalVariables();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("a", result[0]);
        }

        [TestMethod]
        public void Equals()
        {
            AssignClassVarExpression cmd1 = new("a", new ConstantExpression(1));
            AssignClassVarExpression cmd2 = new("a", new ConstantExpression(2));
            AssignClassVarExpression cmd3 = new("b", new ConstantExpression(1));
            AssignClassVarExpression cmd4 = new("a", new ConstantExpression(1));

            Assert.IsTrue(cmd1.Equals(cmd4));
            Assert.IsTrue(cmd4.Equals(cmd1));
            Assert.AreEqual(cmd1.GetHashCode(), cmd4.GetHashCode());

            Assert.IsFalse(cmd1.Equals(null));
            Assert.IsFalse(cmd1.Equals(cmd2));
            Assert.IsFalse(cmd1.Equals(cmd3));
            Assert.IsFalse(cmd1.Equals(123));
        }
    }
}
