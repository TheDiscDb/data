namespace TheDiscDb.Imdb;

public class ImageData
{
    public string IMDbId { get; set; }
    public string Title { get; set; }
    public string FullTitle { get; set; }
    public string Type { get; set; }
    public string Year { get; set; }
    public List<ImageDataDetail> Items { get; set; }
    public string ErrorMessage { get; set; }

    public ImageData()
    {
        ErrorMessage = string.Empty;
        Items = new List<ImageDataDetail>();
    }

    public ImageData(string id, string errorMessage)
    {
        IMDbId = id;
        ErrorMessage = errorMessage;
        Items = null;
    }
}
