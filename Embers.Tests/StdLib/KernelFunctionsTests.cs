using Embers.Exceptions;
using System.IO;

namespace Embers.Tests.StdLib
{
    [TestClass]
    public class KernelFunctionsTests
    {
        [TestMethod]
        public void Sleep_ReturnsNil()
        {
            Machine machine = new();
            var result = machine.ExecuteText("sleep(0)");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Gets_ReadsLine()
        {
            Machine machine = new();
            var originalIn = Console.In;
            try
            {
                Console.SetIn(new StringReader("hello\n"));
                var result = machine.ExecuteText("gets");
                Assert.AreEqual("hello", result);
            }
            finally
            {
                Console.SetIn(originalIn);
            }
        }

        [TestMethod]
        public void Exit_RaisesEmbersError()
        {
            Machine machine = new();
            try
            {
                machine.ExecuteText("exit");
                Assert.Fail("Expected EmbersError");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(EmbersError));
                Assert.AreEqual("exit", ex.Message);
            }
        }
    }
}
