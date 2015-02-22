using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace XmppBot.Common
{
    public class XmppBotConfig
    {
        public string Server { get; set; }

        public string Rooms { get; set; }

        public string RoomNick { get; set; }

        public string Password { get; set; }

        public string User { get; set; }

        public string Resource { get; set; }

        public string ConferenceServer { get; set; }

        public static XmppBotConfig FromAppConfig()
        {
            var config = new XmppBotConfig();

            config.Server = ConfigurationManager.AppSettings["Server"];
            config.Rooms = ConfigurationManager.AppSettings["Rooms"];
            config.RoomNick = ConfigurationManager.AppSettings["RoomNick"];
            config.Password = ConfigurationManager.AppSettings["Password"];
            config.User = ConfigurationManager.AppSettings["User"];
            config.Resource = ConfigurationManager.AppSettings["Resource"];
            config.ConferenceServer = ConfigurationManager.AppSettings["ConferenceServer"];

            return config;
        }
    }
}
