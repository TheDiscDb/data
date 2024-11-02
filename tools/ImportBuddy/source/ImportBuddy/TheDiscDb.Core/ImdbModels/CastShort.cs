namespace TheDiscDb.Imdb;

public record CastShort
{
    public CastShort()
    {
    }

    public CastShort(string job)
    {
        Job = job;
    }

    public string Job { get; set; } = string.Empty;
    public List<CastShortItem> Items { get; set; } = new();
}
