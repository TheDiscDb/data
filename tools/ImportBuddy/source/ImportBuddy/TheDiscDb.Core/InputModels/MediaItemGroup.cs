namespace TheDiscDb.InputModels
{
    public class MediaItemGroup
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public int Id { get; set; }
        public int MediaItemId { get; set; }
        public int GroupId { get; set; }
        public string Role { get; set; }
        public bool IsFeatured { get; set; }

        [HotChocolate.Data.UseFiltering]
        [HotChocolate.Data.UseSorting]
        public MediaItem MediaItem { get; set; }

        [HotChocolate.Data.UseFiltering]
        [HotChocolate.Data.UseSorting]
        public Group Group { get; set; }
    }
}
