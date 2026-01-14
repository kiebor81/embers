namespace Embers.Tests.Compiler
{
    using Embers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ParserModernHashSyntaxTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup() => machine = new Machine();

        [TestMethod]
        public void ModernHashSyntaxBasic()
        {
            var result = machine.ExecuteText("{a: 1, b: 2}");
            Assert.IsNotNull(result);
            Assert.AreEqual("{:a=>1, :b=>2}", result.ToString());
        }

        [TestMethod]
        public void ModernHashSyntaxAccessValue()
        {
            var result = machine.ExecuteText("h = {name: \"Alice\", age: 30}; h[:name]");
            Assert.AreEqual("Alice", result);
        }

        [TestMethod]
        public void ModernHashSyntaxWithStrings()
        {
            var result = machine.ExecuteText("{name: \"Alice\", city: \"NYC\"}");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void OldHashSyntaxStillWorks()
        {
            var result = machine.ExecuteText("{:a => 1, :b => 2}");
            Assert.IsNotNull(result);
            Assert.AreEqual("{:a=>1, :b=>2}", result.ToString());
        }

        [TestMethod]
        public void OldHashSyntaxWithMixedKeys()
        {
            var result = machine.ExecuteText("{\"name\" => \"Alice\", :age => 30}");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ModernSyntaxEqualsOldSyntax()
        {
            var modern = machine.ExecuteText("{a: 1, b: 2}");
            var old = machine.ExecuteText("{:a => 1, :b => 2}");
            Assert.AreEqual(modern.ToString(), old.ToString());
        }

        [TestMethod]
        public void ModernSyntaxWithNonNameKeyThrowsError() => Assert.ThrowsException<Embers.Exceptions.SyntaxError>(() =>
                                                                        {
                                                                            machine.ExecuteText("{\"name\": \"Alice\"}");
                                                                        });

        [TestMethod]
        public void ModernSyntaxEmptyHash()
        {
            var result = machine.ExecuteText("{}");
            Assert.IsNotNull(result);
            Assert.AreEqual("{}", result.ToString());
        }

        [TestMethod]
        public void ModernSyntaxWithNestedHash()
        {
            var result = machine.ExecuteText("{outer: {inner: 1}}");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ModernSyntaxWithHashMethods()
        {
            var result = machine.ExecuteText("{a: 1, b: 2}.keys");
            Assert.IsNotNull(result);
        }
    }
}
