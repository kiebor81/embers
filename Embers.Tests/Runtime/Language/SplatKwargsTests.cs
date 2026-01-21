using Embers.Exceptions;

namespace Embers.Tests.Runtime.Language
{
    [TestClass]
    public class SplatKwargsTests
    {
        private Machine machine;

        [TestInitialize]
        public void Setup() => machine = new Machine();

        [TestMethod]
        public void DefWithSplatCapturesArgs()
        {
            var result = machine.ExecuteText("def foo(*args); args.length; end; foo(1, 2, 3)");
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void DefWithKwargsCapturesHash()
        {
            var result = machine.ExecuteText("def foo(**kwargs); kwargs.size; end; foo(**{a: 1, b: 2})");
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void ForwardingPreservesArgsKwargsAndBlock()
        {
            var result = machine.ExecuteText(@"
def target(*args, **kwargs, &block)
  block.call(args.length + kwargs.size)
end
def wrapper(*args, **kwargs, &block)
  target(*args, **kwargs, &block)
end
wrapper(1, 2, **{a: 1, b: 2}) { |n| n + 1 }
");
            Assert.AreEqual(5L, result);
        }

        [TestMethod]
        public void KeywordSplatRequiresHash()
        {
            Assert.ThrowsException<TypeError>(() => machine.ExecuteText("def foo(**kwargs); end; foo(**1)"));
        }
    }
}
