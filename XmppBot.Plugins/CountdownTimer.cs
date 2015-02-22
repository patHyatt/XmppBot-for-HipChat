using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using XmppBot.Common;

namespace XmppBot.Plugins
{
    /// <summary>
    /// Adds a command to tell the bot to count down from a specified number of seconds to zero
    /// by a specified interval. 
    /// </summary>
    [Export(typeof(IXmppBotPlugin))]
    public class CountdownTimer : XmppBotPluginBase, IXmppBotPlugin
    {
        public override string EvaluateEx(ParsedLine line)
        {
            if (!line.IsCommand || line.Command.ToLower() != "countdown")
            {
                return null;
            }

            string help = "!countdown [seconds] [interval]";

            // Verify we have enough arguments
            if (line.Args.Length < 2)
            {
                return help;
            }

            int seconds;
            int interval;

            // Parse the arguments
            if (!int.TryParse(line.Args[0], out seconds) || !int.TryParse(line.Args[1], out interval))
            {
                return help;
            }

            // Create an interval sequence that fires off a value every [interval] seconds
            IObservable<string> seq = Observable.Interval(TimeSpan.FromSeconds(interval))

                // Run that seq until the total time has exceeded the [seconds] value
                                                .TakeWhile(l => ((l + 1) * interval) < seconds)

                // Project each element in the sequence to a human-readable time value
                                                .Select(
                                                    l =>
                                                    String.Format("{0} seconds remaining...",
                                                        seconds - ((l + 1) * interval)))


                                                .Concat(Observable.Return("Finished!"));
            seq.Subscribe((msg) =>
            {
                this.SendMessage(msg, line.From, BotMessageType.groupchat);
            });

            return null;
        }

        public override string Name
        {
            get { return "CountdownTimer"; }
        }
    }
}