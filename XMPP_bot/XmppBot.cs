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
using log4net;

namespace XmppBot.Service
{
    class XmppBot
    {
        #region log4net
        private static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        private const int MaxRosterSize = 100;

        private static DirectoryCatalog _catalog = null;
        private static XmppClientConnection _client = null;


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

            log.Info(pluginList);

            _client = new XmppClientConnection(ConfigurationManager.AppSettings["Server"]);

            //_client.ConnectServer = "talk.google.com"; //necessary if connecting to Google Talk
            _client.AutoResolveConnectServer = false;

            _client.OnLogin += new ObjectHandler(xmpp_OnLogin);
            _client.OnMessage += new MessageHandler(xmpp_OnMessage);
            _client.OnError += new ErrorHandler(_client_OnError);

            log.Info("Connecting...");
            _client.Resource = ConfigurationManager.AppSettings["Resource"];
            _client.Open(ConfigurationManager.AppSettings["User"], ConfigurationManager.AppSettings["Password"]);
            log.Info("Connected.");

            _client.OnRosterStart += new ObjectHandler(_client_OnRosterStart);
            _client.OnRosterItem += new XmppClientConnection.RosterHandler(_client_OnRosterItem);
        }

        static void _client_OnError(object sender, Exception ex)
        {
            log.Error("Exception: " + ex);
        }

        static void _catalog_Changed(object sender, ComposablePartCatalogChangeEventArgs e)
        {
            _catalog.Refresh();
        }

        #region Roster Management

        private static Dictionary<string, IChatUser> _roster = new Dictionary<string, IChatUser>(MaxRosterSize);

        static void _client_OnRosterStart(object sender)
        {
            _roster = new Dictionary<string, IChatUser>(MaxRosterSize);
        }

        static void _client_OnRosterItem(object sender, RosterItem item)
        {
            if (!_roster.ContainsKey(item.Jid.User))
            {
                _addRoster(new ChatUser(item));
            }
        }

        private static void _addRoster(IChatUser user)
        {
            if (!_roster.ContainsKey(user.Bare))
                _roster.Add(user.Bare, user);
        }

        #endregion


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
            if (!string.IsNullOrEmpty(msg.Body))
            {
                log.InfoFormat("Message : {0} - from {1}", msg.Body, msg.From);

                IChatUser user = null;
                _roster.TryGetValue(msg.From.Bare, out user);

                // we can't find a user or this is the bot talking
                if (null == user || ConfigurationManager.AppSettings["RoomNick"] == user.Name)
                    return;

                ParsedLine line = new ParsedLine(msg.Body.Trim(), user.Name);

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