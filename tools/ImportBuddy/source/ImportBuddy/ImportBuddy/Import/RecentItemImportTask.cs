using Fantastic.FileSystem;
using Fantastic.TheMovieDb.Models;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace ImportBuddy;

public class RecentItemImportTask : IImportTask
{
    private readonly IFileSystem fileSystem;
    private readonly IOptions<ImportBuddyOptions> options;

    public RecentItemImportTask(IFileSystem fileSystem, IOptions<ImportBuddyOptions> options)
    {
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public bool CanHandle(string title, string itemType)
    {
        if (string.IsNullOrEmpty(title))
        {
            return false;
        }

        return title.Equals("x"); // case sensitive
    }

    public async Task<ImportItem> GetImportItem(string title, string itemType, CancellationToken cancellationToken = default)
    {
        IEnumerable<KeyValuePair<string, DateTimeOffset>> dirs;
        const int choiceCount = 3;
        if (itemType.Equals("Series", StringComparison.OrdinalIgnoreCase))
        {
            string inputDirectory = this.fileSystem.Path.Combine(this.options.Value.DataRepositoryPath, "Series");
            dirs = (await this.GetSortedDirectories(inputDirectory, cancellationToken)).Take(choiceCount);
        }
        else
        {
            string inputDirectory = this.fileSystem.Path.Combine(this.options.Value.DataRepositoryPath, itemType);
            dirs = (await this.GetSortedDirectories(inputDirectory, cancellationToken)).Take(choiceCount);
        }

        var choices = new SelectionPrompt<string>();
        var fullTitles = new List<ImportItem>();

        int i = 0;
        foreach (var dir in dirs)
        {
            string tmdbPath = this.fileSystem.Path.Combine(dir.Key, "tmdb.json");

            if (await this.fileSystem.File.Exists(tmdbPath, cancellationToken))
            {
                Series series = null;
                Movie movie = null;

                string displayTitle = string.Empty;
                string displayYear = string.Empty;

                var newItem = new ImportItem
                {
                    Type = itemType
                };

                if (itemType.Equals("Series", StringComparison.OrdinalIgnoreCase))
                {
                    newItem.Series = await this.fileSystem.Deserialize<Series>(tmdbPath, cancellationToken);
                    displayTitle = newItem.Series?.Name;
                    displayYear = newItem.Series.FirstAirDate?.Year.ToString() ?? "";
                }
                else
                {
                    newItem.Movie = await this.fileSystem.Deserialize<Movie>(tmdbPath, cancellationToken);
                    displayTitle = newItem.Movie?.Title;
                    displayYear = newItem.Movie.ReleaseDate?.Year.ToString() ?? "";
                }

                fullTitles.Add(newItem);
                choices.AddChoice($"{i++}: {displayTitle} ({displayYear})");
            }
        }

        string choice = AnsiConsole.Prompt(choices);
        int index = Int32.Parse(choice[0].ToString());
        var chosenFullTitle = fullTitles[index];

        return chosenFullTitle;
    }

    private async Task<IOrderedEnumerable<KeyValuePair<string, DateTimeOffset>>> GetSortedDirectories(string inputDirectory, CancellationToken cancellationToken)
    {
        var results = new Dictionary<string, DateTimeOffset>();
        var directories = await this.fileSystem.Directory.GetDirectories(inputDirectory, cancellationToken);
        foreach (var directory in directories)
        {
            var info = await this.fileSystem.Directory.GetDirectoryInfo(directory);
            results[directory] = info.LastWriteTimeUtc;
        }

        var sorted = results.OrderByDescending(p => p.Value);
        return sorted;
    }
}
