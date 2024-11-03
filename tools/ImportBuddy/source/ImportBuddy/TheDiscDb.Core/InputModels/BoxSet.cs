namespace TheDiscDb.InputModels;

using System.Text.Json.Serialization;

public class Boxset : IDisplayItem
{
    [JsonIgnore]
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? SortTitle { get; set; }
    public string? Slug { get; set; }
    public string? ImageUrl { get; set; }
    public Release? Release { get; set; }
    [JsonIgnore]
    public int ReleaseId { get; set; }
    [JsonIgnore]
    public string Type => "Boxset";
}
