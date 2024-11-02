namespace TheDiscDb.InputModels
{
    using System;
    using System.Collections.Generic;

    public class MediaItem : IDisplayItem
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string FullTitle { get; set; }
        public string SortTitle { get; set; }
        public int Year { get; set; }
        public string Type { get; set; }
        public string ImageUrl { get; set; }
        public ExternalIds Externalids { get; set; } = new ExternalIds();
        [System.Text.Json.Serialization.JsonIgnore]
        public int ExternalIdsId { get; set; }

        [HotChocolate.Data.UseFiltering]
        [HotChocolate.Data.UseSorting]
        public ICollection<Release> Releases { get; set; } = new HashSet<Release>();

        [HotChocolate.Data.UseFiltering]
        [HotChocolate.Data.UseSorting]
        public ICollection<MediaItemGroup> MediaItemGroups { get; set; } = new HashSet<MediaItemGroup>();

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
        public DateTimeOffset LatestReleaseDate { get; set; }
        public DateTimeOffset DateAdded { get; set; }
    }
}
