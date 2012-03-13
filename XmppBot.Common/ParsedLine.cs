using System.Linq;

namespace XmppBot.Common
{
    public class ParsedLine
    {
        public ParsedLine(string line, string user)
        {
            ParseLine(line);
            this.User = user;
        }

        private void ParseLine(string line)
        {
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
        public string User { get; private set; }
    }
}
