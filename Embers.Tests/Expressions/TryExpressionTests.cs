using Embers.StdLib;

namespace Embers.Tests.Expressions
{
    [TestClass]
    public class TryExpressionTests
    {
        [TestMethod]
        public void TryRescueEnsure_CatchesExceptionAndRunsEnsure()
        {
            // Arrange
            var machine = new Machine();
            var output = new StringWriter();
            machine.RootContext.Self.Class.SetInstanceMethod("puts", new PutsFunction(output));

            string code = @"
                begin
                  puts 1
                  raise 'fail'
                  puts 2
                rescue
                  puts 'rescued'
                ensure
                  puts 'ensured'
                end
            ";

            // Act
            machine.ExecuteText(code);

            // Assert
            var result = output.ToString().Replace("\r\n", "\n"); // Normalize line endings
            Assert.AreEqual("1\nrescued\nensured\n", result);
        }

        [TestMethod]
        public void TryRescueEnsure_NoExceptionStillRunsEnsure()
        {
            var machine = new Machine();
            var output = new StringWriter();
            machine.RootContext.Self.Class.SetInstanceMethod("puts", new PutsFunction(output));

            string code = @"
                begin
                  puts 1
                rescue ex
                  puts 'rescued'
                ensure
                  puts 'ensured'
                end
            ";

            machine.ExecuteText(code);

            var result = output.ToString().Replace("\r\n", "\n");
            Assert.AreEqual("1\nensured\n", result);
        }
    }
}
