namespace TheDiscDb.Imdb;

public class SimilarShort
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Image { get; set; }
    public string IMDbRating { get; set; }

    public SimilarShort()
    {
        Id = (Title = (Image = (IMDbRating = string.Empty)));
    }
}
