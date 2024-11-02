namespace MakeMkv
{
    public class SourceInformationLogLine : LogLine
    {
        //CINFO:1,6209,"Blu-ray disc"
        public SourceInformationLogLine() : base("CINFO")
        {
        }

        public int Code { get; set; }
        public int SubCode { get; set; }
        public string Message { get; set; }

        public static SourceInformationLogLine Parse(string line)
        {
            string[] parts = line.Substring(6).Split(',');

            return new SourceInformationLogLine
            {
                Code = TryParseInt(0, parts),
                SubCode = TryParseInt(1, parts),
                Message = GetString(2, parts),
                OriginalLine = line
            };
        }
    }
}
