using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmppBot.Common
{
    public interface IChatUser
    {
        string Name { get; }

        string Id { get; }

        string Bare { get; }

        string Mention { get; }
    }
}
