using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

using XmppBot.Common;

namespace XmppBot.Plugins
{
    [Export(typeof(IXmppBotPlugin))]
    public class Example : IXmppBotPlugin
    {
        public string Evaluate(ParsedLine line)
        {
            if (!line.IsCommand) return string.Empty;

            switch (line.Command.ToLower())
            {
                case "smack":
                    return String.Format("{0} smacks {1} around with a large trout", line.User, line.Args.FirstOrDefault() ?? "your mom");

                case "hug":
                    return String.Format("{0} hugs {1}", line.User, line.Args.FirstOrDefault() ?? "themself");

                default:
                    return null;
            }
        }

        public string Name
        {
            get { return "User Actions"; }
        }
    }
}
