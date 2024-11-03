namespace TheDiscDb.Imdb;

public class PosterData
{
    public string? IMDbId { get; set; }

    public string? Title { get; set; }

    public string? FullTitle { get; set; }

    public string? Type { get; set; }

    public string? Year { get; set; }

    public List<PosterDataItem> Posters { get; set; } = new List<PosterDataItem>();

    public List<PosterDataItem> Backdrops { get; set; } = new List<PosterDataItem>();

    public string? ErrorMessage { get; set; }
}
