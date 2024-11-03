namespace TheDiscDb.ImportModels
{
    using System.Collections.Generic;
    using TheDiscDb.InputModels;

    public class DiscFileItem
    {
        public string? Title { get; set; }
        public string? SourceFile { get; set; }
        public string? SegmentMap { get; set; }
        public string? Duration { get; set; }
        public string? Size { get; set; }
        public string? Comment { get; set; }
        public string? Type { get; set; }

        public string? Episode { get; set; }
        public string? Season { get; set; }
        public int Year { get; set; }
        public string? Upc { get; set; }
        public string? Description { get; set; }

        public ICollection<Chapter> Chapters { get; set; } = new HashSet<Chapter>();
    }
}
