using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmppBot.Common
{
    public class XmppBotCommand
    {
        public XmppBotCommand() { }

        public XmppBotCommand(string helpInfo, PluginMethod method)
        {
            this.HelpInfo = HelpInfo;
            this.Method = method;
        }

        

        public string HelpInfo { get; set; }

        public PluginMethod Method { get; set; }

        public delegate string PluginMethod(ParsedLine line);
    }

    
}
