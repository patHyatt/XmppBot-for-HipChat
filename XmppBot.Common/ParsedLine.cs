using Ader.Text;
using System.Collections.Generic;
using System.Linq;

namespace XmppBot.Common
{
    public class ParsedLine
    {
        public ParsedLine(string from, string line, string room, IChatUser user, BotMessageType messageType)
        {
            this.User = user;
            this.Tokens = new Tokens();

            ParseLine(from, room, line, messageType);
        }

        private void ParseLine(string from, string room, string line, BotMessageType messageType)
        {
            this.Room = room;
            this.From = from;
            this.Raw = line;
            this.MessageType = messageType;
            this.IsCommand = line.StartsWith("!");
            line = line.TrimStart('!');

            string[] parts = line.Split(' ');
            if (parts.Length <= 0)
            {
                this.Command = "invalid";
                this.Args = new string[] { };
                this.IsCommand = false;
                return;
            }

            this.Command = parts[0];
            this.Args = parts.Skip(1).ToArray();

            TokenizeLine(line);
        }

        private void TokenizeLine(string line)
        {
            StringTokenizer tokenizer = new StringTokenizer(line) { IgnoreWhiteSpace = true, RemoveQuotes = true };

            Token token;
            int tokenIndex = 0;
            this.Tokens.Args = new List<string>();
            do
            {
                token = tokenizer.Next();
                switch (tokenIndex++)
                {
                    case 0:
                        {
                            this.Tokens.Command = token.Value;
                            break;
                        }
                    default:
                        {
                            if (!string.IsNullOrWhiteSpace(token.Value))
                            {
                                this.Tokens.Args.Add(token.Value.TrimStart(new char[] { '\\' }).TrimEnd(new char[] { '\\' }));
                            }
                            break;
                        }
                }
            } while (token.Kind != TokenKind.EOF);

        }

        public string Command { get; private set; }

        public string Raw { get; private set; }

        public string[] Args { get; private set; }

        public bool IsCommand { get; private set; }

        public IChatUser User { get; private set; }

        public string From { get; private set; }

        public string Room { get; private set; }

        public BotMessageType MessageType { get; private set; }

        public Tokens Tokens { get; set; }
    }
}
