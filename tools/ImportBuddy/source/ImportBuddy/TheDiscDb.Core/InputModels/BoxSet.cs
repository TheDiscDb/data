namespace TheDiscDb.InputModels;

using System.Text.Json.Serialization;

public class Boxset : IDisplayItem
{
    [JsonIgnore]
    public int Id { get; set; }
    public string Title { get; set; }
    public string SortTitle { get; set; }
    public string Slug { get; set; }
    public string ImageUrl { get; set; }
    public Release Release { get; set; }
    [JsonIgnore]
    public int ReleaseId { get; set; }
    [JsonIgnore]
    public string Type => "Boxset";
}

public interface IDisplayItem
{
    public string Title { get; }
    public string Slug { get; }
    public string ImageUrl { get; }
    public string Type { get; }
}

public interface IPageInfo
{
    public bool HasNextPage { get; }

    public bool HasPreviousPage { get; }

    public string? StartCursor { get; }

    public string? EndCursor { get; }
}