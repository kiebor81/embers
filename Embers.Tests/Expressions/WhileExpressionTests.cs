using Embers.Expressions;

namespace Embers.Tests.Expressions
{
    [TestClass]
    public class WhileExpressionTests
    {
        [TestMethod]
        public void ExecuteSimpleWhileWhenConditionIsTrue()
        {
            Parser cmdparser = new("a = a + 1");
            IExpression body = cmdparser.ParseCommand();
            Parser exprparser = new("a < 6");
            IExpression expr = exprparser.ParseExpression();

            Context context = new();
            context.SetLocalValue("a", 1);

            WhileExpression cmd = new(expr, body);

            cmd.Evaluate(context);

            Assert.AreEqual(6L, context.GetValue("a"));
        }

        [TestMethod]
        public void Equals()
        {
            WhileExpression cmd1 = new(new ConstantExpression(1), new AssignExpression("one", new ConstantExpression(1)));
            WhileExpression cmd2 = new(new ConstantExpression(2), new AssignExpression("one", new ConstantExpression(1)));
            WhileExpression cmd3 = new(new ConstantExpression(1), new AssignExpression("one", new ConstantExpression(2)));
            WhileExpression cmd4 = new(new ConstantExpression(1), new AssignExpression("one", new ConstantExpression(1)));

            Assert.IsTrue(cmd1.Equals(cmd4));
            Assert.IsTrue(cmd4.Equals(cmd1));
            Assert.AreEqual(cmd1.GetHashCode(), cmd4.GetHashCode());

            Assert.IsFalse(cmd1.Equals(null));
            Assert.IsFalse(cmd1.Equals(123));
            Assert.IsFalse(cmd1.Equals(cmd2));
            Assert.IsFalse(cmd1.Equals(cmd3));
        }
    }
}
