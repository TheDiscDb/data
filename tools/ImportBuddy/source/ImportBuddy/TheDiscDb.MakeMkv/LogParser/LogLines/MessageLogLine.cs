namespace MakeMkv
{
    using System.Collections.Generic;

    public class MessageLogLine : LogLine
    {
        //MSG:3307,0,2,"File 00006.mpls was added as title #0","File %1 was added as title #%2","00006.mpls","0"
        public MessageLogLine() : base("MSG")
        {
        }

        public string? Code { get; set; }
        public string? Flags { get; set; }
        public int ParamCount { get; set; }
        public string? Message { get; set; }
        public string? MessageTemplate { get; set; }
        public IList<string> Params { get; set; } = new List<string>();

        public static MessageLogLine Parse(string line)
        {
            var enumerator = new CsvEnumerator(line[4..]);

            var result = new MessageLogLine
            {
                Code = GetString(enumerator),
                Flags = GetString(enumerator),
                ParamCount = TryParseInt(enumerator),
                Message = GetString(enumerator),
                MessageTemplate = GetString(enumerator),
                OriginalLine = line
            };

            while (GetString(enumerator) is {} val)
                result.Params.Add(val);

            return result;
        }
    }
}
