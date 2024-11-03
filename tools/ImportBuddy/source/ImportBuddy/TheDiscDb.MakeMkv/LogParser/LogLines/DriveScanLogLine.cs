namespace MakeMkv
{
    public class DriveScanLogLine : LogLine
    {
        //DRV:1,2,999,12,"BD-ROM HL-DT-ST BDDVDRW UH12NS30 1.03","MR SELFRIDGE SEASON 2 DISC 1","E:"
        public DriveScanLogLine() : base("DRV")
        {
        }

        public int Index { get; set; }
        public bool Visible { get; set; }
        public bool Enabled { get; set; }
        public string? Flags { get; set; }
        public string? DriveName { get; set; }
        public string? DiscName { get; set; }
        public string? DriveLetter { get; set; }

        public static DriveScanLogLine Parse(string line)
        {
            string[] parts = line.Substring(3).Split(',');

            return new DriveScanLogLine
            {
                Index = TryParseInt(0, parts),
                Visible = TryParseBoolean(1, parts),
                Enabled = TryParseBoolean(2, parts),
                Flags = GetString(3, parts),
                DriveName = GetString(4, parts),
                DiscName = GetString(5, parts),
                DriveLetter = GetString(6, parts)
            };
        }
    }
}
