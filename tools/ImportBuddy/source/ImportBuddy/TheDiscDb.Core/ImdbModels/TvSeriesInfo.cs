namespace TheDiscDb.Imdb;

public class TvSeriesInfo
{
    public string YearEnd { get; set; }
    public string Creators { get; set; }
    public List<StarShort> CreatorList { get; set; }
    public List<string> Seasons { get; set; }

    public TvSeriesInfo()
    {
        CreatorList = new List<StarShort>();
        Seasons = new List<string>();
        Creators = (YearEnd = string.Empty);
    }
}
