using Embers.Compiler;
using Embers.Exceptions;

namespace Embers.Tests.Compiler;

[TestClass]
public class ParserObserverTests
{
    private sealed class RecordingObserver : IParserObserver
    {
        public List<ParserTokenInfo> Tokens { get; } = new();
        public List<ParserCommandInfo> CommandEnters { get; } = new();
        public List<ParserCommandInfo> CommandExits { get; } = new();
        public List<ParserErrorInfo> Errors { get; } = new();

        public void OnTokenRead(ParserTokenInfo token) => Tokens.Add(token);
        public void OnCommandEnter(ParserCommandInfo command) => CommandEnters.Add(command);
        public void OnCommandExit(ParserCommandInfo command) => CommandExits.Add(command);
        public void OnParseError(ParserErrorInfo error) => Errors.Add(error);
    }

    [TestMethod]
    public void ObserverReceivesTokenAndCommandEvents()
    {
        Parser parser = new("a=1\n");
        RecordingObserver observer = new();
        parser.Observer = observer;

        var result = parser.ParseCommand();

        Assert.IsNotNull(result);
        Assert.AreEqual(1, observer.CommandEnters.Count);
        Assert.AreEqual(1, observer.CommandExits.Count);
        Assert.AreEqual(observer.CommandEnters[0].CommandIndex, observer.CommandExits[0].CommandIndex);
        Assert.IsNotNull(observer.CommandEnters[0].Span);

        var nameToken = observer.Tokens.First(t => t.Type == TokenType.Name && t.Value == "a");
        var assignToken = observer.Tokens.First(t => t.Type == TokenType.Operator && t.Value == "=");
        var valueToken = observer.Tokens.First(t => t.Type == TokenType.Integer && t.Value == "1");

        Assert.IsNotNull(nameToken.Span);
        Assert.IsNotNull(assignToken.Span);
        Assert.IsNotNull(valueToken.Span);
        Assert.AreEqual(0, nameToken.Span!.Value.StartOffset);
        Assert.AreEqual(1, nameToken.Span!.Value.EndOffset);
        Assert.AreEqual(1, assignToken.Span!.Value.StartOffset);
        Assert.AreEqual(2, assignToken.Span!.Value.EndOffset);
        Assert.AreEqual(2, valueToken.Span!.Value.StartOffset);
        Assert.AreEqual(3, valueToken.Span!.Value.EndOffset);
    }

    [TestMethod]
    public void ObserverReceivesParseError()
    {
        Parser parser = new("*");
        RecordingObserver observer = new();
        parser.Observer = observer;

        try
        {
            parser.ParseCommand();
            Assert.Fail("Expected a SyntaxError.");
        }
        catch (SyntaxError ex)
        {
            Assert.AreEqual(1, observer.CommandEnters.Count);
            Assert.AreEqual(0, observer.CommandExits.Count);
            Assert.AreEqual(1, observer.Errors.Count);
            Assert.AreEqual(ex.Message, observer.Errors[0].Message);
            Assert.AreEqual(0, observer.Errors[0].CommandIndex);
            Assert.IsNotNull(observer.Errors[0].Span);
            Assert.AreEqual(0, observer.Errors[0].Span!.Value.StartOffset);
            Assert.AreEqual(1, observer.Errors[0].Span!.Value.EndOffset);
        }
    }
}
