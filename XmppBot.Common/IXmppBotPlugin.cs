using System;
using System.Collections.Generic;

namespace XmppBot.Common
{
    public interface IXmppBotPlugin
    {
        void Initialize();

        string Evaluate(ParsedLine line);

        string Help(ParsedLine line);

        string Name { get; }

        bool Enabled { get; set; }

        // this event fires when the bot sends a message to a client
        event PluginMessageHandler SentMessage;
        
        Dictionary<string, XmppBotCommand> Commands { get; }
    }
}
