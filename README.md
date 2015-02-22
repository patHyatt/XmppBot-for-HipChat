To use:
Copy/Paste the App.config.template file and rename to App.config. Inside the newly named App.config file, fill in key entries with appropriate values from your XMPP page in hipchat.


To add functionality:
Implement the IXmppBotPlugin interface and decorate the class with [Export(typeof(IXmppBotPlugin))].
Implement the interfaces methods (Evaluate and Name).
Uberfy your HipChat.

Example:

```c#
using System;
using System.ComponentModel.Composition;
using System.Linq;

using XmppBot.Common;

namespace XmppBot.Extensions
{
    [Export(typeof(IXmppBotPlugin))]
    public class SmackEm : IXmppBotPlugin
    {
        public string Evaluate(ParsedLine line)
        {
            if (!line.IsCommand)
                return "";

            switch (line.Command.ToLower())
            {
                case "smack":
                    return String.Format("{0} smacks {1} around with a trout.", line.User, line.Args.FirstOrDefault() ?? "Your mom");

                default: return null;
            }
        }

        public string Name
        {
            get { return "Smack Em!"; }
        }
    }
}
```

You can run the bot as a console application, or you can install it as a Windows Service by running: 

	XmppBot.Service.exe install

For more info about installing as a service, see the [TopShelf documentation](http://docs.topshelf-project.com/en/latest/overview/commandline.html).