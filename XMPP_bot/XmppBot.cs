using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using XmppBot.Common;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.roster;
using agsXMPP.protocol.x.muc;

namespace XMPP_bot
{
    class XmppBot
    {
        private static DirectoryCatalog _catalog = null;
        private static XmppClientConnection _client = null;
        private static Dictionary<string, string> _roster = new Dictionary<string, string>(20);

        public void Stop()
        {
        }

        public void Start()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (o, args) =>
            {
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                return loadedAssemblies.FirstOrDefault(asm => asm.FullName == args.Name);
            };

            string pluginsDirectory = Environment.CurrentDirectory + "\\plugins\\";
            if (!Directory.Exists(pluginsDirectory))
            {
                Directory.CreateDirectory(pluginsDirectory);
            }

            _catalog = new DirectoryCatalog(Environment.CurrentDirectory + "\\plugins\\");
            _catalog.Changed += new EventHandler<ComposablePartCatalogChangeEventArgs>(_catalog_Changed);
            var pluginList = LoadPlugins();

            Console.WriteLine(pluginList);

            _client = new XmppClientConnection(ConfigurationManager.AppSettings["Server"]);

            //_client.ConnectServer = "talk.google.com"; //necessary if connecting to Google Talk
            _client.AutoResolveConnectServer = false;

            _client.OnLogin += new ObjectHandler(xmpp_OnLogin);
            _client.OnMessage += new MessageHandler(xmpp_OnMessage);
            _client.OnError += new ErrorHandler(_client_OnError);

            Console.WriteLine("Connecting...");
            _client.Resource = ConfigurationManager.AppSettings["Resource"];
            _client.Open(ConfigurationManager.AppSettings["User"], ConfigurationManager.AppSettings["Password"]);
            Console.WriteLine("Connected.");

            _client.OnRosterStart += new ObjectHandler(_client_OnRosterStart);
            _client.OnRosterItem += new XmppClientConnection.RosterHandler(_client_OnRosterItem);
        }

        static void _client_OnError(object sender, Exception ex)
        {
            Console.WriteLine("Exception: " + ex);
        }

        static void _catalog_Changed(object sender, ComposablePartCatalogChangeEventArgs e)
        {
            _catalog.Refresh();
        }

        static void _client_OnRosterStart(object sender)
        {
            _roster = new Dictionary<string, string>(20);
        }

        static void _client_OnRosterItem(object sender, RosterItem item)
        {
            if (!_roster.ContainsKey(item.Jid.User))
                _roster.Add(item.Jid.User, item.Name);
        }

        static void xmpp_OnLogin(object sender)
        {
            MucManager mucManager = new MucManager(_client);

            string[] rooms = ConfigurationManager.AppSettings["Rooms"].Split(',');

            foreach (string room in rooms)
            {
                Jid jid = new Jid(room + "@" + ConfigurationManager.AppSettings["ConferenceServer"]);
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
                        SendMessage(msg.From, "I'm a quitter...", msg.Type);
                        Environment.Exit(1);
                        return;

                    case "reload":
                        SendMessage(msg.From, LoadPlugins(), msg.Type);
                        break;

                    default:
                        Task.Factory.StartNew(() =>
                                              Parallel.ForEach(Plugins,
                                                  plugin => SendMessage(msg.From, plugin.Evaluate(line), msg.Type)
                                                  ));

                        Task.Factory.StartNew(() =>
                                              Parallel.ForEach(SequencePlugins,
                                                  plugin => SendSequence(msg.From, plugin.Evaluate(line), msg.Type)
                                                  ));
                        break;
                }
            }
        }

        public static void SendMessage(Jid to, string text, MessageType type)
        {
            if (text == null) return;

            _client.Send(new Message(to, type, text));
        }

        public static void SendSequence(Jid to, IObservable<string> messages, MessageType type)
        {
            if(messages == null)
            {
                return;
            }

            var observer = Observer.Create<string>(
                msg => SendMessage(to, msg, type),
                exception => Trace.TraceError(exception.ToString()));

            messages.Subscribe(observer);
        }

        [ImportMany(AllowRecomposition = true)]
        public static IEnumerable<IXmppBotPlugin> Plugins { get; set; }

        [ImportMany(AllowRecomposition = true)]
        public static IEnumerable<IXmppBotSequencePlugin> SequencePlugins { get; set; }

        private static string LoadPlugins()
        {
            var container = new CompositionContainer(_catalog);
            Plugins = container.GetExportedValues<IXmppBotPlugin>();
            SequencePlugins = container.GetExportedValues<IXmppBotSequencePlugin>();

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