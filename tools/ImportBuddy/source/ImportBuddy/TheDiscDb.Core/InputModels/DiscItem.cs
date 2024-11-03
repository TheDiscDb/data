namespace TheDiscDb.InputModels
{
    using System.Collections.Generic;

    public interface IDiscItem
    {
        string? SourceFile { get; }
        string? Description { get; }
        string? ItemType { get; }
        string? Season { get; }
        string? Episode { get; }
        string? SegmentMap { get; }
        string? Duration { get; }
        string? DisplaySize { get; }
        bool HasItem { get; }
    }

    public class DiscItem : IDiscItem
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public int Id { get; set; }
        public string? Comment { get; set; }
        public string? SourceFile { get; set; }
        public string? SegmentMap { get; set; }
        public string? Duration { get; set; }
        public long Size { get; set; }
        public string? DisplaySize { get; set; }

        public DiscItemReference? Item { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public int? DiscItemReferenceId { get; set; }

        [HotChocolate.Data.UseFiltering]
        [HotChocolate.Data.UseSorting]
        public ICollection<Track> Tracks { get; set; } = new HashSet<Track>();

        [System.Text.Json.Serialization.JsonIgnore]
        public string Description => this.Item?.Title ?? string.Empty;
        [System.Text.Json.Serialization.JsonIgnore]
        public string ItemType => this.Item?.Type ?? string.Empty;
        [System.Text.Json.Serialization.JsonIgnore]
        public string Season => this.Item?.Season ?? string.Empty;
        [System.Text.Json.Serialization.JsonIgnore]
        public string Episode => this.Item?.Episode ?? string.Empty;
        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasItem => this.Item != null;
    }
}
