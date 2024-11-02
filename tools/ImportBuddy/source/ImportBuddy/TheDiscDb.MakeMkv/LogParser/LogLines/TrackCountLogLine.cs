namespace MakeMkv
{
    using System;

    public class TrackCountLogLine : LogLine
    {
        //TCOUNT:4
        public TrackCountLogLine() : base("TCOUNT")
        {
        }

        public int Count { get; set; }

        public static TrackCountLogLine Parse(string line)
        {
            int count = 0;
            Int32.TryParse(line.Substring(7), out count);

            return new TrackCountLogLine
            {
                Count = count,
                OriginalLine = line
            };
        }
    }
}
