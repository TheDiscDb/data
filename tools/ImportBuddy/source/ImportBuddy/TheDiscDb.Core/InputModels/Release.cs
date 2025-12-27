namespace TheDiscDb.InputModels
{
    using System;
    using System.Collections.Generic;

    public class Release : IDisplayItem
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public int Id { get; set; }
        public string? Slug { get; set; }
        public string? Title { get; set; }
        public string? RegionCode { get; set; }
        public string? Locale { get; set; }
        public int Year { get; set; }
        public string? Upc { get; set; }
        public string? Isbn { get; set; }
        public string? Asin { get; set; }
        public string? ImageUrl { get; set; }
        public DateTimeOffset ReleaseDate { get; set; }
        public DateTimeOffset DateAdded { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string FullTitle => $"{Title} ({Year})";

        [System.Text.Json.Serialization.JsonIgnore]
        public string Type => "Release"; // never actually used

        [HotChocolate.Data.UseFiltering]
        [HotChocolate.Data.UseSorting]
        public ICollection<Disc> Discs { get; set; } = new HashSet<Disc>();
        [System.Text.Json.Serialization.JsonIgnore]
        public MediaItem? MediaItem { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Boxset? Boxset { get; set; }
        public ICollection<Contributor> Contributors { get; set; } = new List<Contributor>();
    }
}
