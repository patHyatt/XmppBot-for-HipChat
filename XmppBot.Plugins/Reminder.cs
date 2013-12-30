using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using XmppBot.Common;

namespace XmppBot.Plugins
{
    /// <summary>
    /// Adds a command to tell the bot to remind you of something at a specified time
    /// </summary>
    [Export(typeof(IXmppBotSequencePlugin))]
    public class Reminder : IXmppBotSequencePlugin
    {
        public IObservable<string> Evaluate(ParsedLine line)
        {
            if(!line.IsCommand || line.Command.ToLower() != "reminder")
            {
                return null;
            }

            string help = "!reminder [time] [message]";

            // Verify we have enough arguments
            if(line.Args.Length < 2)
            {
                return Observable.Return(help);
            }

            DateTimeOffset time;

            // Parse the arguments
            if(!DateTimeOffset.TryParse(line.Args[0], out time))
            {
                return Observable.Return(help);
            }

            // We want anything entered after the time to be included in the reminder
            string message = line.Args.Skip(1).Aggregate(String.Empty, (s, s1) => s + (s.Length == 0 ? "" : " ") + s1);

            // Create an sequence that fires off single value at a specified time
            // and transform that value into the reminder message
            IObservable<string> seq = Observable.Timer(time).Select(l => message);

            // Add a start message
            return Observable.Return(String.Format("Will do - I'll remind you at {0}.", time))
                             .Concat(seq);
        }

        public string Name
        {
            get { return "Reminder"; }
        }
    }
}