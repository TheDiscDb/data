namespace MakeMkv
{
    public class SegmentInformationLogLine : LogLine
    {
        //SINFO:0,0,5,0,"V_MPEG4/ISO/AVC"
        public SegmentInformationLogLine() : base("SINFO")
        {
        }

        public int TrackIndex { get; set; }
        public int SegmentIndex { get; set; }
        public int Code { get; set; }
        public int SubCode { get; set; }
        public string? Message { get; set; }

        public static SegmentInformationLogLine Parse(string line)
        {
            string[] parts = line.Substring(5).Split(',');

            return new SegmentInformationLogLine
            {
                TrackIndex = TryParseInt(0, parts),
                SegmentIndex = TryParseInt(1, parts),
                Code = TryParseInt(2, parts),
                SubCode = TryParseInt(3, parts),
                Message = GetString(4, parts),
                OriginalLine = line
            };
        }
    }
}
