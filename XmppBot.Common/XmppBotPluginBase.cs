using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmppBot.Common
{
    /// <summary>
    /// Delegate for handling plugin message sends.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="jid"></param>
    /// <param name="messageType"></param>
    public delegate void PluginMessageHandler(string text, string jid, BotMessageType messageType);

    public abstract class XmppBotPluginBase : IXmppBotPlugin
    {
        public event PluginMessageHandler SentMessage;

        Dictionary<string, XmppBotCommand> _commands = new Dictionary<string, XmppBotCommand>();

        public XmppBotPluginBase()
        {
            this.Enabled = this.EnabledByDefault;
        }

        public virtual void Initialize() { }

        public virtual string Help(ParsedLine line)
        {
            if (this.Commands.Count <= 0)
                return "";

            if (line.Args.Length == 0)
                return String.Format("{0} commands: {1}", this.Name, String.Join(",", this.Commands.Keys.ToArray()));

            XmppBotCommand command = GetCommand(line.Args.FirstOrDefault());

            return command == null ? "" : command.HelpInfo;
        }

        public string Evaluate(ParsedLine line)
        {
            if (!this.Enabled)
            {
                System.Diagnostics.Debug.WriteLine("{0} is disabled, re-enable by typing !enable {0}", new[] { this.Name });
                return null;
            }

            return EvaluateEx(line);
        }

        public virtual string EvaluateEx(ParsedLine line)
        {
            if (!line.IsCommand)
                return null; // this is not a command

            XmppBotCommand command = GetCommand(line.Command);

            return command == null ? null : command.Method(line);
        }

        protected XmppBotCommand GetCommand(string commandText)
        {
            XmppBotCommand cmd = null;
            this.Commands.TryGetValue(commandText, out cmd);

            return cmd;
        }

        /// <summary>
        /// Actually send a message.
        /// </summary>
        protected void SendMessage(string text, string jid, BotMessageType messageType)
        {
            this.SentMessage(text, jid, messageType);
        }

        /// <summary>
        /// Name of this plugin.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// List of commands supported by this plugin.
        /// </summary>
        public Dictionary<string, XmppBotCommand> Commands
        {
            get
            {
                return _commands;
            }
        }

        /// <summary>
        /// Is this plugin currently enabled?
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Should this plugin start out as enabled?
        /// </summary>
        public virtual bool EnabledByDefault { get { return true; } }
    }
}
