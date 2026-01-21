using Embers.Exceptions;
using Embers.Expressions;
using Embers.Functions;

namespace Embers.Tests.Expressions
{
    [TestClass]
    public class NameExpressionTests
    {
        [TestMethod]
        public void EvaluateUndefinedName()
        {
            NameExpression expr = new("foo");
            Context context = new();

            try
            {
                expr.Evaluate(context);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(NameError));
                Assert.AreEqual("undefined local variable or method 'foo'", ex.Message);
            }
        }

        [TestMethod]
        public void EvaluateUndefinedConstant()
        {
            NameExpression expr = new("Foo");
            Context context = new();

            try
            {
                expr.Evaluate(context);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(NameError));
                Assert.AreEqual("unitialized constant Foo", ex.Message);
            }
        }

        [TestMethod]
        public void EvaluateDefinedName()
        {
            NameExpression expr = new("one");
            Context context = new();
            context.SetLocalValue("one", 1);

            Assert.AreEqual(1, expr.Evaluate(context));
        }

        [TestMethod]
        public void EvaluateDefinedFunction()
        {
            Machine machine = new();
            NameExpression expr = new("foo");
            Context context = machine.RootContext;
            context.Self.Class.SetInstanceMethod("foo", new DefinedFunction(new ConstantExpression(1), [], null, null, context));

            Assert.AreEqual(1, expr.Evaluate(context));
        }

        [TestMethod]
        public void NamedExpression()
        {
            INamedExpression expr = new NameExpression("foo");

            Assert.AreSame("foo", expr.Name);
            Assert.IsNull(expr.TargetExpression);
        }

        [TestMethod]
        public void Equals()
        {
            NameExpression expr1 = new("one");
            NameExpression expr2 = new("two");
            NameExpression expr3 = new("one");

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
