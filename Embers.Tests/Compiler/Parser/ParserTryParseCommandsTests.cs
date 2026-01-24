namespace Embers.Tests.Compiler;

[TestClass]
public class ParserTryParseCommandsTests
{
    [TestMethod]
    public void TryParseCommands_ReturnsPartialCommandsAndErrorMessage()
    {
        string text = "a=1\n(1+2";

        var commands = Parser.TryParseCommands(text, out var errorMessage);

        Assert.AreEqual(1, commands.Count);
        Assert.AreEqual("expected ')'", errorMessage);
    }

    [TestMethod]
    public void TryParseCommands_ReturnsAllCommandsWhenNoErrors()
    {
        string text = "a=1\nb=2";

        var commands = Parser.TryParseCommands(text, out var errorMessage);

        Assert.AreEqual(2, commands.Count);
        Assert.IsNull(errorMessage);
    }
}
