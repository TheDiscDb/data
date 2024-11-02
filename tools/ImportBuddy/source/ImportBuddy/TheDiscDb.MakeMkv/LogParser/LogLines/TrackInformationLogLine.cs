namespace MakeMkv
{
    public class TrackInformationLogLine : LogLine
    {
        //TINFO:0,10,0,"31.0 GB"
        public TrackInformationLogLine() : base("TINFO")
        {
        }

        public int Index { get; set; }
        public int Code { get; set; }
        public int SubCode { get; set; }
        public string Message { get; set; }

        public static TrackInformationLogLine Parse(string line)
        {
            int openQuote = line.IndexOf('"');
            string codePart = line.Substring(6, openQuote - 6);
            string messagePart = line.Substring(openQuote + 1, line.Length - (openQuote + 2));
            string[] parts = codePart.Split(',');

            return new TrackInformationLogLine
            {
                Index = TryParseInt(0, parts),
                Code = TryParseInt(1, parts),
                SubCode = TryParseInt(2, parts),
                Message = messagePart,
                OriginalLine = line
            };
        }
    }
}
