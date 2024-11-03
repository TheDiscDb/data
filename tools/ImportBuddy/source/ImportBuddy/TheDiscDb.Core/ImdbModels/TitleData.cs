namespace TheDiscDb.Imdb;

public class TitleData
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? OriginalTitle { get; set; }
    public string? FullTitle { get; set; }
    public string? Type { get; set; }
    public string? Year { get; set; }
    public string? Image { get; set; }
    public string? ReleaseDate { get; set; }
    public string? RuntimeMins { get; set; }
    public string? RuntimeStr { get; set; }
    public string? Plot { get; set; }
    public string? PlotLocal { get; set; }
    public bool PlotLocalIsRtl { get; set; }
    public string? Awards { get; set; }
    public string? Directors { get; set; }
    public List<StarShort> DirectorList { get; set; } = new();
    public string? Writers { get; set; }
    public List<StarShort> WriterList { get; set; } = new();
    public string? Stars { get; set; }
    public List<StarShort> StarList { get; set; } = new();
    public List<ActorShort> ActorList { get; set; } = new();
    public FullCastData FullCast { get; set; } = new();
    public string? Genres { get; set; }
    public List<KeyValueItem> GenreList { get; set; } = new();
    public string? Companies { get; set; }
    public List<CompanyShort> CompanyList { get; set; } = new();
    public string? Countries { get; set; }
    public List<KeyValueItem> CountryList { get; set; } = new();
    public string? Languages { get; set; }
    public List<KeyValueItem> LanguageList { get; set; } = new();
    public string? ContentRating { get; set; }
    public string? IMDbRating { get; set; }
    public string? IMDbRatingVotes { get; set; }
    public string? MetacriticRating { get; set; }
    public RatingData Ratings { get; set; } = new();
    public WikipediaData Wikipedia { get; set; } = new();
    public PosterData Posters { get; set; } = new();
    public ImageData Images { get; set; } = new();
    public TrailerData Trailer { get; set; } = new();
    public BoxOfficeShort BoxOffice { get; set; } = new();
    public string? Tagline { get; set; }
    public string? Keywords { get; set; }
    public List<string> KeywordList { get; set; } = new();
    public List<SimilarShort> Similars { get; set; } = new();
    public TvSeriesInfo TvSeriesInfo { get; set; } = new();
    public TvEpisodeInfo TvEpisodeInfo { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
