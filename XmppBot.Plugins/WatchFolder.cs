using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using XmppBot.Common;

namespace XmppBot.Plugins
{
    /// <summary>
    /// Adds commands to tell the bot to watch a folder and send chat 
    /// messages when files are created, renamed, or deleted. 
    /// </summary>
    [Export(typeof(IXmppBotSequencePlugin))]
    public class WatchFolder : IXmppBotSequencePlugin
    {
        // This will act as a way to funnel unwatch requests into the observable streams
        private readonly Subject<string> _unwatch = new Subject<string>();
        
        public IObservable<string> Evaluate(ParsedLine line)
        {
            if(line.IsCommand && line.Command.ToLower() == "unwatch")
            {
                if(line.Args.Length < 1)
                {
                    return Observable.Return("!unwatch [path]");
                }

                // Request that this path no longer be watched
                _unwatch.OnNext(line.Args[0]);

                return null;
            }

            if (!line.IsCommand || line.Command.ToLower() != "watchfolder")
            {
                return null;
            }

            string help = "!watch [path]";

            // Verify we have enough arguments
            if(line.Args.Length < 1)
            {
                return Observable.Return(help);
            }

            string path = line.Args[0];

            try
            {
                // Create a file system watcher for the requested folder
                var fsw = new FileSystemWatcher(path);
                fsw.EnableRaisingEvents = true;

                // Create observables for the Created, Deleted, and Renamed events from the file system watcher
                // Each one takes the events and converts them to a string description of the event
                var obsCreated = fsw.CreateObservableForCreated().Select(args => String.Format("{0} was created", args.Name));
                var obsDeleted = fsw.CreateObservableForDeleted().Select(args => String.Format("{0} was deleted", args.Name));
                var obsRenamed = fsw.CreateObservableForRenamed().Select(args => String.Format("{0} was renamed", args.Name));

                // Merge all the events into a single stream
                var changes = obsCreated.Merge(obsDeleted).Merge(obsRenamed)
                                        .Merge(_unwatch.Where(s => s.Contains(path))) // Merge with the stream of unwatch requests which match this path
                                        .TakeWhile(args => args != path); // end the sequence when we get a string that just matches the path (which would be from the unwatch)
                                                        
                // Add a start message and return the observable stream
                return Observable.Return(String.Format("Watching for changes to {0}.", path))
                                 .Concat(changes)
                                 .Concat(Observable.Return(string.Format("Done watching {0}.", path)));
            }
            catch(Exception ex)
            {
                // Show the error in the chat - most likely it'll be an invalid path issue
                return Observable.Return(ex.Message);
            }
        }

        public string Name
        {
            get { return "WatchFolder"; }
        }
    }

    /// <summary>
    /// These extensions just get some of the event -> observable wiring out of the way
    /// </summary>
    public static class FileSystemWatcherObservableExtensions
    {
        public static IObservable<FileSystemEventArgs> CreateObservableForCreated(this FileSystemWatcher fsw)
        {
            return Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                handler => (sender, e) => handler(e),
                handler => fsw.Created += handler,
                handler => fsw.Created -= handler);
        }

        public static IObservable<FileSystemEventArgs> CreateObservableForDeleted(this FileSystemWatcher fsw)
        {
            return Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                handler => (sender, e) => handler(e),
                handler => fsw.Deleted += handler,
                handler => fsw.Deleted -= handler);
        }

        public static IObservable<FileSystemEventArgs> CreateObservableForRenamed(this FileSystemWatcher fsw)
        {
            return Observable.FromEvent<RenamedEventHandler, FileSystemEventArgs>(
                handler => (sender, e) => handler(e),
                handler => fsw.Renamed += handler,
                handler => fsw.Renamed -= handler);
        }
    }
}