using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XmppBot.Common
{
    public abstract class XmppBotQueuedPluginBase : XmppBotPluginBase
    {

        #region Command Queue

        private BlockingCollection<QueuedCommand> _commandQueue;
        private Thread _checkThread;

        #endregion

        public XmppBotQueuedPluginBase()
            : base()
        {
            _commandQueue = new BlockingCollection<QueuedCommand>(100);

            // A simple blocking consumer with no cancellation
            _checkThread = new Thread(CheckForCommands);
            _checkThread.Start();
        }

        private void CheckForCommands()
        {
            while (!_commandQueue.IsCompleted)
            {
                QueuedCommand command = null;
                // Blocks if number.Count == 0 
                // IOE means that Take() was called on a completed collection. 
                // Some other thread can call CompleteAdding after we pass the 
                // IsCompleted check but before we call Take.  
                // In this example, we can simply catch the exception since the  
                // loop will break on the next iteration. 
                try
                {
                    command = _commandQueue.Take();
                }
                catch (InvalidOperationException) { }

                if (command != null)
                {
                    string result = command.Command.Method(command.Line);

                    this.SendMessage(result, command.Line.Room, command.Line.MessageType);
                }
            }
        }
        
        public override string EvaluateEx(ParsedLine line)
        {
            if (!line.IsCommand)
                return null; // this is not a command

            XmppBotCommand command = GetCommand(line.Command);

            // this is a command for this plugin, so we are going to queue it up
            if (command != null)
            {
                _commandQueue.Add(new QueuedCommand(line, command));
                return string.Format("Queued up: {0}", line.Command);
            }

            return null; // nothing to do
        }

    }
}
