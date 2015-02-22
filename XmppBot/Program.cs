using System;
using System.Linq;
using Topshelf;
using XmppBot.Common;

namespace XmppBot.Service
{
    public class Program
    {
        public static void Main()
        {
            HostFactory.Run(x =>                                 
            {
                x.Service<Bot>(s =>
                    {
                        s.ConstructUsing(name => new Bot(XmppBotConfig.FromAppConfig()));
                        s.WhenStarted(xmppbot => xmppbot.Start());   
                        s.WhenStopped(xmppbot => xmppbot.Stop());     
                    });

                x.RunAsLocalSystem();                            

                x.SetDescription("XmppBot");        
                x.SetDisplayName("XmppBot");                       
                x.SetServiceName("XmppBot");                       
            });

            Console.ReadLine();
        }
    }
}
