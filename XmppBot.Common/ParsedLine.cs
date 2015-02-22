using System.Linq;

namespace XmppBot.Common
{
    public class ParsedLine
    {
        public ParsedLine(string from, string line, IChatUser user)
        {
            ParseLine(from, line);
            this.User = user;
        }

        private void ParseLine(string from, string line)
        {
            this.From = from;
            this.Raw = line;
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
        }

        public string Command { get; private set; }

        public string Raw { get; private set; }

        public string[] Args { get; private set; }

        public bool IsCommand { get; private set; }

        public IChatUser User { get; private set; }

        public string From { get; private set; }
    }
}
