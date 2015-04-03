using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmppBot.Common
{
    internal class QueuedCommand
    {
        public QueuedCommand(ParsedLine line, XmppBotCommand command)
        {
            this.Line = line;
            this.Command = command;
        }

        public ParsedLine Line { get; set; }

        public XmppBotCommand Command { get; set; }
    }
}
