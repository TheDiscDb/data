namespace TheDiscDb.Imdb;

public class TitleData : ICloneable
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string OriginalTitle { get; set; }
    public string FullTitle { get; set; }
    public string Type { get; set; }
    public string Year { get; set; }
    public string Image { get; set; }
    public string ReleaseDate { get; set; }
    public string RuntimeMins { get; set; }
    public string RuntimeStr { get; set; }
    public string Plot { get; set; }
    public string PlotLocal { get; set; }
    public bool PlotLocalIsRtl { get; set; }
    public string Awards { get; set; }
    public string Directors { get; set; }
    public List<StarShort> DirectorList { get; set; }
    public string Writers { get; set; }
    public List<StarShort> WriterList { get; set; }
    public string Stars { get; set; }
    public List<StarShort> StarList { get; set; }
    public List<ActorShort> ActorList { get; set; }
    public FullCastData FullCast { get; set; }
    public string Genres { get; set; }
    public List<KeyValueItem> GenreList { get; set; }
    public string Companies { get; set; }
    public List<CompanyShort> CompanyList { get; set; }
    public string Countries { get; set; }
    public List<KeyValueItem> CountryList { get; set; }
    public string Languages { get; set; }
    public List<KeyValueItem> LanguageList { get; set; }
    public string ContentRating { get; set; }
    public string IMDbRating { get; set; }
    public string IMDbRatingVotes { get; set; }
    public string MetacriticRating { get; set; }
    public RatingData Ratings { get; set; }
    public WikipediaData Wikipedia { get; set; }
    public PosterData Posters { get; set; }
    public ImageData Images { get; set; }
    public TrailerData Trailer { get; set; }
    public BoxOfficeShort BoxOffice { get; set; }
    public string Tagline { get; set; }
    public string Keywords { get; set; }
    public List<string> KeywordList { get; set; }
    public List<SimilarShort> Similars { get; set; }
    public TvSeriesInfo TvSeriesInfo { get; set; }
    public TvEpisodeInfo TvEpisodeInfo { get; set; }
    public string ErrorMessage { get; set; }

    public TitleData()
    {
        ErrorMessage = string.Empty;
        DirectorList = new List<StarShort>();
        WriterList = new List<StarShort>();
        StarList = new List<StarShort>();
        ActorList = new List<ActorShort>();
        FullCast = new FullCastData();
        GenreList = new List<KeyValueItem>();
        CompanyList = new List<CompanyShort>();
        CountryList = new List<KeyValueItem>();
        LanguageList = new List<KeyValueItem>();
        Posters = new PosterData();
        Images = new ImageData();
        KeywordList = new List<string>();
        BoxOffice = new BoxOfficeShort();
        Similars = new List<SimilarShort>();
        TvSeriesInfo = new TvSeriesInfo();
        TvEpisodeInfo = new TvEpisodeInfo();
        Ratings = new RatingData();
        Wikipedia = new WikipediaData();
        Writers = (Directors = (Stars = (Companies = (Countries = (Genres = (Keywords = (Languages = string.Empty)))))));
    }

    public TitleData(string id, string errorMessage)
    {
        ErrorMessage = errorMessage;
        Id = id;
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
