namespace TheDiscDb.InputModels;

public interface IDisplayItem
{
    public string? Title { get; }
    public string? Slug { get; }
    public string? ImageUrl { get; }
    public string? Type { get; }
}
