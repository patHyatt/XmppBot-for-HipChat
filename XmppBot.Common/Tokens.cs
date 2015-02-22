using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmppBot.Common
{
    public class Tokens
    {
        public Tokens()
        {
            this.Args = new List<string>();
        }

        public List<string> Args { get; set; }

        public string Command { get; set; }
    }
}
