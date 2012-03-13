namespace XmppBot.Common
{
    public interface IXmppBotPlugin
    {
        string Evaluate(ParsedLine line);
        string Name { get; }
    }
}
