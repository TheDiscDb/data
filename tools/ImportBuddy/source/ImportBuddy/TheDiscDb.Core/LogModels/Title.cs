namespace TheDiscDb.LogModels
{
    using System.Collections.Generic;
    using TheDiscDb.InputModels;

    public class Title
    {
        public int Index { get; set; }
        public string Length { get; set; }
        public string DisplaySize { get; set; }
        public long Size { get; set; }
        public string Playlist { get; set; }
        public string SegmentMap { get; set; }
        public string Comment { get; set; }

        public IList<Track> Tracks { get; set; } = new List<Track>();
        public DiscItemReference Episode { get; set; }
        public DiscItemReference Extra { get; set; }
    }
}
