namespace TheDiscDb.Imdb;

public record FullCastData
{
    public string IMDbId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string FullTitle { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public CastShort Directors { get; set; } = new CastShort("Director");
    public CastShort Writers { get; set; } = new CastShort("Writer");
    public List<ActorShort> Actors { get; set; } = new List<ActorShort>();
    public List<ActorShort> Others { get; set; } = new List<ActorShort>();

    public FullCastData()
    {
    }

    public FullCastData(string errorMessage)
    {
        this.ErrorMessage = errorMessage;
    }

    public FullCastData(string id, string errorMessage)
        : this(errorMessage)
    {
        this.IMDbId = id;
    }
}