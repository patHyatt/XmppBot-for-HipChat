using agsXMPP.protocol.iq.roster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmppBot.Common
{
    public class ChatUser : IChatUser
    {
        public string Name { get; private set; }

        public string Id { get; private set; }

        public string Bare { get; private set; }

        public string Mention { get; private set; }

        private ChatUser()
        {
            this.Name = string.Empty;
            this.Id = string.Empty;
            this.Bare = string.Empty;
            this.Mention = string.Empty;
        }

        public ChatUser(RosterItem item)
        {
            this.Name = item.Name;
            this.Mention = item.Name;
            this.Id = item.Jid.User;
            this.Bare = item.Jid.Bare;

            try
            {
                var mention = item.Attributes["mention_name"].ToString();
                if (!String.IsNullOrWhiteSpace(mention))
                    this.Mention = mention;
            }
            catch { }
        }
    }
}
