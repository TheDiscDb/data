namespace MakeMkv
{
    using System.Collections.Generic;

    public class MessageLogLine : LogLine
    {
        //MSG:3307,0,2,"File 00006.mpls was added as title #0","File %1 was added as title #%2","00006.mpls","0"
        public MessageLogLine() : base("MSG")
        {
        }

        public string Code { get; set; }
        public string Flags { get; set; }
        public int ParamCount { get; set; }
        public string Message { get; set; }
        public string MessageTemplate { get; set; }
        public IList<string> Params { get; set; } = new List<string>();

        public static MessageLogLine Parse(string line)
        {
            string[] parts = line.Substring(4).Split(',');

            var result = new MessageLogLine
            {
                Code = GetString(0, parts),
                Flags = GetString(1, parts),
                ParamCount = TryParseInt(2, parts),
                Message = GetString(3, parts),
                MessageTemplate = GetString(4, parts),
                OriginalLine = line
            };

            for (int i = 5; i < parts.Length; i++)
            {
                result.Params.Add(GetString(i, parts));
            }

            return result;
        }
    }
}
