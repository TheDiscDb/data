namespace TheDiscDb.Imdb;

public class PosterData
{
    public string IMDbId { get; set; }

    public string Title { get; set; }

    public string FullTitle { get; set; }

    public string Type { get; set; }

    public string Year { get; set; }

    public List<PosterDataItem> Posters { get; set; }

    public List<PosterDataItem> Backdrops { get; set; }

    public string ErrorMessage { get; set; }

    public PosterData()
    {
        ErrorMessage = string.Empty;
        Posters = new List<PosterDataItem>();
        Backdrops = new List<PosterDataItem>();
    }

    public PosterData(string id, string errorMessage)
    {
        ErrorMessage = errorMessage;
        IMDbId = id;
        Posters = new List<PosterDataItem>();
        Backdrops = new List<PosterDataItem>();
    }

    public PosterData(string errorMessage)
    {
        ErrorMessage = errorMessage;
        Posters = new List<PosterDataItem>();
        Backdrops = new List<PosterDataItem>();
    }
}
