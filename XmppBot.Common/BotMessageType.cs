using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmppBot.Common
{
    public enum BotMessageType
    {
        normal = -1,
        error = 0,
        chat = 1,
        groupchat = 2,
        headline = 3,
    }
}
