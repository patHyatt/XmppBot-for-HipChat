using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

using XmppBot.Common;

namespace XmppBot.Plugins
{
    [Export(typeof(IXmppBotPlugin))]
    public class Example : XmppBotPluginBase, IXmppBotPlugin
    {
        public override string EvaluateEx(ParsedLine line)
        {
            if (!line.IsCommand) return string.Empty;

            switch (line.Command.ToLower())
            {
                case "smack":
                    return String.Format("{0} smacks {1} around with a large trout", line.User, line.Args.FirstOrDefault() ?? "your mom");

                case "hug":
                    return String.Format("{0} hugs {1}", line.User, line.Args.FirstOrDefault() ?? "themself");

                case "help":
                    return String.Format("Right now the only commands I know are !smack [thing] and !hug [thing].");

                default:
                    return null;
            }
        }

        public override string Name
        {
            get { return "User Actions"; }
        }
    }
}
