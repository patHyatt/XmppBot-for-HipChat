using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmppBot.Common
{
    public abstract class XmppBotConfirmedPluginBase : XmppBotPluginBase
    {
        #region Command History

        private ConcurrentDictionary<string, ParsedLine> commandQueue = new ConcurrentDictionary<string, ParsedLine>();

        private string AreYouSure(ParsedLine line)
        {
            ClearCommandHistory(line.User.Id);
            commandQueue.TryAdd(line.User.Id, line);

            return string.Format("Are you sure you want to run: '{0}'?", line.Raw);
        }

        private string ExecuteConfirm(ParsedLine line)
        {
            if (commandQueue.ContainsKey(line.User.Id))
            {
                var command = commandQueue[line.User.Id];
                return base.EvaluateEx(command); // just execute it now that it's confirmed
            }
            else
            {
                return null;
            }
        }

        public override string EvaluateEx(ParsedLine line)
        {
            // check to see if this is a confirmation
            if (commandQueue.Count > 0)
            {
                if (line.Raw.ToUpperInvariant() == "YES" ||
                    line.Raw.ToUpperInvariant() == "Y")
                {
                    string res = ExecuteConfirm(line);
                    ParsedLine cmdLine;
                    commandQueue.TryRemove(line.User.Id, out cmdLine);

                    if (!string.IsNullOrEmpty(res))
                    {
                        return res;
                    }
                }
                else if (
                  line.Raw.ToUpperInvariant() == "NO" ||
                  line.Raw.ToUpperInvariant() == "N")
                {
                    ClearCommandHistory(line.User.Id);
                }
            }


            // now check for a command
            if (!line.IsCommand)
                return null; // this is not a command

            XmppBotCommand command = GetCommand(line.Command);
            if (command != null)
            {
                // queue this command if it is one
                AreYouSure(line);
            }

            return null;
        }

        private void ClearCommandHistory(string id)
        {
            if (commandQueue.ContainsKey(id))
            {
                ParsedLine command;
                commandQueue.TryRemove(id, out command);
                SendMessage(string.Format("I am not going to execute the last command you asked for: {0}", command.Raw), command.Room, command.MessageType);
            }
        }

        #endregion
    }
}
