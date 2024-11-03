namespace TheDiscDb.InputModels
{
    using System.Collections.Generic;

    public class Group
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public int Id { get; set; }
        public string? ImdbId { get; set; }
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public string? ImageUrl { get; set; }

        [HotChocolate.Data.UseFiltering]
        [HotChocolate.Data.UseSorting]
        public ICollection<MediaItemGroup> MediaItemGroups { get; set; } = new HashSet<MediaItemGroup>();
    }
}
