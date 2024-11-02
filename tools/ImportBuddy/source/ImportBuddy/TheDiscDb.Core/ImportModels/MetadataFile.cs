namespace TheDiscDb.ImportModels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TheDiscDb.InputModels;

    public class MetadataFile
    {
        public const string Filename = "metadata.json";

        public string Title { get; set; }
        public string FullTitle { get; set; }
        public string SortTitle { get; set; }
        public string Slug { get; set; }
        public string Type { get; set; }
        public int Year { get; set; }
        public string ImageUrl { get; set; }
        public ExternalIds ExternalIds { get; set; } = new ExternalIds();
        public ICollection<string> Groups { get; set; } = new List<string>();

        public string Plot { get; set; }
        public string Tagline { get; set; }
        public string Directors { get; set; }
        public string Writers { get; set; }
        public string Stars { get; set; }
        public string Genres { get; set; }
        public int RuntimeMinutes { get; set; }
        public string Runtime { get; set; }
        public string ContentRating { get; set; }
        public DateTimeOffset ReleaseDate { get; set; }
        public DateTimeOffset DateAdded { get; set; }
    }
}
