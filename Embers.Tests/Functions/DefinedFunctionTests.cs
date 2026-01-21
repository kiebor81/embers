using Embers.Expressions;
using Embers.Functions;
using Embers.StdLib;

namespace Embers.Tests.Functions
{
    [TestClass]
    public class DefinedFunctionTests
    {
        [TestMethod]
        public void DefineAndExecuteSimplePuts()
        {
            Machine machine = new();
            StringWriter writer = new();
            PutsFunction puts = new(writer);
            machine.RootContext.Self.Class.SetInstanceMethod("puts", puts);

            DefinedFunction function = new(new CallExpression("puts", [new ConstantExpression(123)]), [], null, null, machine.RootContext);

            Assert.IsNull(function.Apply(machine.RootContext.Self, machine.RootContext, []));
            Assert.AreEqual("123\r\n", writer.ToString());
        }

        [TestMethod]
        public void DefineAndExecuteFunctionWithParameters()
        {
            Context context = new();

            DefinedFunction function = new(new AddExpression(new NameExpression("a"), new NameExpression("b")), ["a", "b"], null, null, context);

            var result = function.Apply(null, null, [1, 2]);

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result);
        }
    }
}
