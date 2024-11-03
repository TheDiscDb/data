namespace TheDiscDb.Imdb;

public class ImageData
{
    public string? IMDbId { get; set; }
    public string? Title { get; set; }
    public string? FullTitle { get; set; }
    public string? Type { get; set; }
    public string? Year { get; set; }
    public List<ImageDataDetail> Items { get; set; } = new List<ImageDataDetail>();
    public string? ErrorMessage { get; set; }
}
