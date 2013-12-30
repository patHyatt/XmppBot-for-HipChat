using System;
using System.Reactive.Linq;

namespace XmppBot.Common
{
    public interface IXmppBotSequencePlugin
    {
        IObservable<string> Evaluate(ParsedLine line);
        string Name { get; }
    }

  
}