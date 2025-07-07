
using TheDiscDb.Imdb;

namespace ImportBuddy;

public class ImportItem
{
    public required ImportItemType Type { get; set; }
    public TitleData? ImdbTitle { get; set; }
    public Fantastic.TheMovieDb.Models.Series? Series { get; set; }
    public Fantastic.TheMovieDb.Models.Movie? Movie { get; set; }
    public object? GetTmdbItemToSerialize()
    {
        if (Type == ImportItemType.Movie)
        {
            return this.Movie;
        }
        else if (Type == ImportItemType.Series)
        {
            return this.Series;
        }

        return null;
    }

    public string? GetPosterUrl()
    {
        if (this.ImdbTitle != null)
        {
            return this.ImdbTitle?.Image;
        }

        if (Type == ImportItemType.Movie)
        {
            return "https://image.tmdb.org/t/p/w500" + this.Movie?.PosterPath;
        }
        else if (Type == ImportItemType.Series)
        {
            return "https://image.tmdb.org/t/p/w500" + this.Series?.PosterPath;
        }

        return null;
    }

    public int TryGetYear()
    {
        if (this.ImdbTitle != null)
        {
            if (int.TryParse(this.ImdbTitle.Year, out int year))
            {
                return year;
            }
        }

        if (this.Movie != null)
        {
            if (this.Movie.ReleaseDate.HasValue)
            {
                return this.Movie.ReleaseDate.Value.Year;
            }
        }

        if (this.Series != null)
        {
            if (this.Series.FirstAirDate.HasValue)
            {
                return this.Series.FirstAirDate.Value.Year;
            }
        }

        return 0;
    }
}
