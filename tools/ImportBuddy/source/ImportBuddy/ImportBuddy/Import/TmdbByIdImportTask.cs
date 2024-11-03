using Fantastic.TheMovieDb;

namespace ImportBuddy;

public class TmdbByIdImportTask : IImportTask
{
    private readonly TheMovieDbClient tmdb;

    public TmdbByIdImportTask(TheMovieDbClient tmdb)
    {
        this.tmdb = tmdb ?? throw new ArgumentNullException(nameof(tmdb));
    }

    public bool CanHandle(string title, string itemType)
    {
        if (string.IsNullOrEmpty(title))
        {
            return false;
        }

        return title.StartsWith("tmdb:", StringComparison.OrdinalIgnoreCase) || Int32.TryParse(title, out var _); // allow just the id to be passed
    }

    public async Task<ImportItem?> GetImportItem(string title, string itemType, CancellationToken cancellationToken = default)
    {
        if (Int32.TryParse(title.Replace("tmdb:", "", StringComparison.OrdinalIgnoreCase), out int id))
        {
            var result = new ImportItem
            {
                Type = itemType
            };

            if (itemType.Equals("Series", StringComparison.OrdinalIgnoreCase))
            {
                result.Series = await this.tmdb.GetSeries(id.ToString(), cancellationToken: cancellationToken);
            }
            else
            {
                result.Movie = await this.tmdb.GetMovie(id.ToString(), cancellationToken: cancellationToken);
            }

            return result;
        }

        return null;
    }
}
