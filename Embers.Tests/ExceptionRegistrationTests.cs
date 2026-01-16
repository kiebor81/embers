using Embers.Exceptions;

namespace Embers.Tests
{
    [TestClass]
    public class ExceptionRegistrationTests
    {
        [TestMethod]
        public void RegistersAllEmbersExceptionsInRootContext()
        {
            Machine machine = new();
            var context = machine.RootContext;

            foreach (var type in GetEmbersExceptionTypes())
            {
                var value = context.GetLocalValue(type.Name);
                Assert.IsNotNull(value, $"Expected {type.Name} to be registered.");
                Assert.AreSame(type, value);
            }
        }

        [TestMethod]
        public void RaiseByNameThrowsCorrectExceptionType()
        {
            Machine machine = new();

            foreach (var type in GetEmbersExceptionTypes())
            {
                try
                {
                    machine.ExecuteText($"raise {type.Name}, 'boom'");
                    Assert.Fail($"Expected {type.Name} to be thrown.");
                }
                catch (Exception ex)
                {
                    Assert.IsInstanceOfType(ex, type, $"Expected {type.Name}, got {ex.GetType().Name}.");
                }
            }
        }

        [TestMethod]
        public void RaiseNoMethodErrorFormatsMessage()
        {
            Machine machine = new();

            try
            {
                machine.ExecuteText("raise NoMethodError, 'missing'");
                Assert.Fail("Expected NoMethodError to be thrown.");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(NoMethodError));
                Assert.AreEqual("undefined method 'missing'", ex.Message);
            }
        }

        private static IEnumerable<Type> GetEmbersExceptionTypes()
        {
            var baseType = typeof(BaseError);
            return baseType.Assembly
                .GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && baseType.IsAssignableFrom(type))
                .OrderBy(type => type.Name);
        }
    }
}
