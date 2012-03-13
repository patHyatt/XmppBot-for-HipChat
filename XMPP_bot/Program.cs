using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.roster;
using agsXMPP.protocol.x.muc;

using XmppBot.Common;
using System.Timers;

namespace XMPP_bot
{
    class Program
    {
        private static DirectoryCatalog _catalog = null;
        private static XmppClientConnection _client = null;
        private static Dictionary<string, string> _roster = new Dictionary<string, string>(20);
        private static Dictionary<string, string> _tempRoster = new Dictionary<string, string>(20);

        static void Main(string[] args)
        {
            _catalog = new DirectoryCatalog(Environment.CurrentDirectory + "\\plugins\\");
            _catalog.Changed += new EventHandler<ComposablePartCatalogChangeEventArgs>(_catalog_Changed);
            LoadPlugins();

            _client = new XmppClientConnection(ConfigurationManager.AppSettings["Server"]);

            //_client.ConnectServer = "talk.google.com"; //necessary if connecting to Google Talk
            _client.AutoResolveConnectServer = false;

            _client.OnLogin += new ObjectHandler(xmpp_OnLogin);
            _client.OnMessage += new MessageHandler(xmpp_OnMessage);

            Console.WriteLine("Connecting...");
            _client.Resource = ConfigurationManager.AppSettings["Resource"];
            _client.Open(ConfigurationManager.AppSettings["User"], ConfigurationManager.AppSettings["Password"]);
            Console.WriteLine("Connected.");

            _client.OnRosterStart += new ObjectHandler(_client_OnRosterStart);
            _client.OnRosterItem += new XmppClientConnection.RosterHandler(_client_OnRosterItem);
            _client.OnRosterEnd += new ObjectHandler(_client_OnRosterEnd);

            while (true)
            {
                System.Threading.Thread.Sleep(200);
            }
        }

        static void _catalog_Changed(object sender, ComposablePartCatalogChangeEventArgs e)
        {
            _catalog.Refresh();
        }

        static void _client_OnRosterStart(object sender)
        {
            _tempRoster = new Dictionary<string, string>(20);
        }

        static void _client_OnRosterEnd(object sender)
        {
            _roster = _tempRoster;
        }

        static void _client_OnRosterItem(object sender, RosterItem item)
        {
            if (!_tempRoster.ContainsKey(item.Jid.User))
                _tempRoster.Add(item.Jid.User, item.Name);
        }

        static void xmpp_OnLogin(object sender)
        {
            MucManager mucManager = new MucManager(_client);

            string[] rooms = ConfigurationManager.AppSettings["Rooms"].Split(',');

            foreach (string room in rooms)
            {
                Jid jid = new Jid(room + "@" + "conf.hipchat.com");
                //mucManager.AcceptDefaultConfiguration(jid);
                mucManager.JoinRoom(jid, ConfigurationManager.AppSettings["RoomNick"]);
            }
        }

        static void xmpp_OnMessage(object sender, Message msg)
        {
            if (!String.IsNullOrEmpty(msg.Body))
            {
                Console.WriteLine("Message : {0} - from {1}", msg.Body, msg.From);

                string user;

                if (msg.Type != MessageType.groupchat)
                {
                    if (!_roster.TryGetValue(msg.From.User, out user))
                    {
                        user = "Unknown User";
                    }
                }
                else
                {
                    user = msg.From.Resource;
                }

                if (user == ConfigurationManager.AppSettings["RoomNick"])
                    return;

                ParsedLine line = new ParsedLine(msg.Body.Trim(), user);

                switch (line.Command)
                {
                    case "close":
                        SendMessage(msg.From.Bare, "I'm a quitter...", msg.Type);
                        Environment.Exit(1);
                        return;

                    case "reload":
                        SendMessage(msg.From.Bare, LoadPlugins(), msg.Type);
                        break;

                    default:
                        Parallel.ForEach<IXmppBotPlugin>(Plugins,
                            plugin => SendMessage(msg.From.Bare, plugin.Evaluate(line), msg.Type)
                            );

                        break;
                }
            }
        }

        public static void SendMessage(Jid to, string text, MessageType type)
        {
            if (text == null) return;

            _client.Send(new Message(to, type, text));
        }

        [ImportMany(AllowRecomposition = true)]
        public static IEnumerable<IXmppBotPlugin> Plugins { get; set; }

        private static string LoadPlugins()
        {
            var container = new CompositionContainer(_catalog);
            Plugins = container.GetExportedValues<IXmppBotPlugin>();

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Loaded the following plugins");
            foreach (var part in _catalog.Parts)
            {
                builder.AppendFormat("\t{0}\n", part.ToString());
            }

            return builder.ToString();
        }
    }
}
