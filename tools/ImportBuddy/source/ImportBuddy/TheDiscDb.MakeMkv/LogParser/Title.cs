namespace MakeMkv
{
    using System.Collections.Generic;

    public class Title
    {
        public const int TrackStart = 2;
        public const int ChapterCountId = 8;
        public const int LengthId = 9;
        public const int DisplaySizeId = 10;
        public const int SizeId = 11;
        public const int PlaylistId = 16;
        public const int SegmentMapId = 26;
        public const int CommentId = 27;
        public const int SourceTitleId = 24; // Used on DVD discs

        public int Index { get; set; }
        public int ChapterCount { get; set; } //8
        public string Length { get; set; } //9
        public string DisplaySize { get; set; } //10
        public long Size { get; set; } //11
        public string Playlist { get; set; } //16
        public string SegmentMap { get; set; } //26
        public string Comment { get; set; } //27
        public IList<Segment> Segments { get; set; } = new List<Segment>();
    }
}
