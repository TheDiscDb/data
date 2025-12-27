namespace TheDiscDb.ImportModels
{
    using System;

    public class ReleaseFile
    {
        public const string Filename = "release.json";

        public string? Slug { get; set; }
        public string? Asin { get; set; }
        public string? Upc { get; set; }
        public int Year { get; set; }
        public string? Locale { get; set; }
        public string? RegionCode { get; set; }
        public string? Title { get; set; }
        public string? SortTitle { get; set; }
        public string? Isbn { get; set; }
        public string? ImageUrl { get; set; }
        public DateTimeOffset ReleaseDate { get; set; }
        public DateTimeOffset DateAdded { get; set; }
        public ICollection<Contributor> Contributors { get; set; } = new HashSet<Contributor>();
    }

    public record Contributor(string Name, string Source);
}
