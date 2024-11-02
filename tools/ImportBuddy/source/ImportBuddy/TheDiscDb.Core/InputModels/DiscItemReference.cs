namespace TheDiscDb.InputModels
{
    using System.Collections.Generic;

    public class DiscItemReference
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        [HotChocolate.Data.UseFiltering]
        [HotChocolate.Data.UseSorting]
        public ICollection<Chapter> Chapters { get; set; } = new HashSet<Chapter>();

        // TODO: How to get this in only for series
        public string Season { get; set; }
        public string Episode { get; set; }

        public Title DiscItem { get; set; }
    }
}
