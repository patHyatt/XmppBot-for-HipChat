using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using XmppBot.Common;

namespace XmppBot.Plugins
{
    [Export(typeof(IXmppBotSequencePlugin))]
    public class CountdownTimer : IXmppBotSequencePlugin
    {
        public IObservable<string> Evaluate(ParsedLine line)
        {
            if (!line.IsCommand || line.Command.ToLower() != "countdown")
            {
                return null;
            }

            string help = "!countdown [seconds] [interval]";

            // Verify we have enough arguments
            if (line.Args.Length < 2)
            {
                return Observable.Return(help);
            }

            int seconds;
            int interval;

            // Parse the arguments
            if (!int.TryParse(line.Args[0], out seconds) || !int.TryParse(line.Args[1], out interval))
            {
                return Observable.Return(help);
            }

            // Create an interval sequence that fires off a value every [interval] seconds
            var seq = Observable.Interval(TimeSpan.FromSeconds(interval))

                // Run that seq until the total time has exceeded the [seconds] value
                                .TakeWhile(l => ((l + 1) * interval) < seconds)

                // Project each element in the sequence to a human-readable time value
                                .Select(l => String.Format("{0} seconds remaining...", seconds - ((l + 1) * interval)));

            // Add a start and end message
            return Observable.Return(String.Format("Starting countdown - {0} seconds remaining...", seconds))
                             .Concat(seq)
                             .Concat(Observable.Return("Finished!"));
        }

        public string Name
        {
            get { return "CountdownTimer"; }
        }
    }
}