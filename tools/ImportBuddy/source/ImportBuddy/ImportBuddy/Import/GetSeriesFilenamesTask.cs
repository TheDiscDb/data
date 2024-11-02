using Fantastic.FileSystem;
using Fantastic.TheMovieDb;
using Fantastic.TheMovieDb.Models;
using Microsoft.Extensions.Options;

namespace ImportBuddy;

public class GetSeriesFilenamesTask
{
    public const string EpisodesFilename = "episodenames.txt";

    private readonly IOptions<ImportBuddyOptions> options;
    private readonly IFileSystem fileSystem;
    private readonly TheMovieDbClient tmdb;

    public GetSeriesFilenamesTask(IOptions<ImportBuddyOptions> options, IFileSystem fileSystem, TheMovieDbClient tmdb)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.tmdb = tmdb;
    }

    public string MenuText => "Get Series Filenames";

    public ushort Id { get; } = 50;

    internal async Task RunInternal(Fantastic.TheMovieDb.Models.Series series, string basePath)
    {
        string episodeListPath = this.fileSystem.Path.Combine(basePath, EpisodesFilename);
        if (!await this.fileSystem.File.Exists(episodeListPath))
        {
            using (var writer = await this.fileSystem.File.CreateText(episodeListPath))
            {
                List<Episode> season0Episodes = new();
                foreach (var season in series.Seasons)
                {
                    var fullSeason = await this.tmdb.GetSeason(series.Id, season.SeasonNumber);

                    if (season.SeasonNumber != 0)
                    {
                        await writer.WriteLineAsync($"------------ Season {season.SeasonNumber:00} -----------");
                        await writer.WriteLineAsync();
                    }

                    foreach (var episode in fullSeason.Episodes)
                    {
                        if (season.SeasonNumber == 0)
                        {
                            season0Episodes.Add(episode);
                            continue;
                        }

                        string fileName = $"{series.Name}.S{season.SeasonNumber:00}.E{episode.EpisodeNumber:00}.{episode.Name}.mkv";
                        fileName = this.fileSystem.CleanPath(fileName);

                        // TODO: Handle multipart episode naming
                        await writer.WriteLineAsync($"Name: {episode.Name}");
                        await writer.WriteLineAsync("Type: Episode");
                        await writer.WriteLineAsync($"Season: {season.SeasonNumber}");
                        await writer.WriteLineAsync($"Episode: {episode.EpisodeNumber}");
                        await writer.WriteLineAsync($"File name: {fileName}");
                        await writer.WriteLineAsync();
                    }

                    await writer.WriteLineAsync();
                }

                // write the season 0 items at the end
                foreach (var episode in season0Episodes)
                {
                    string fileName = $"{series.Name}.S00.E{episode.EpisodeNumber:00}.{episode.Name}.mkv";
                    await writer.WriteLineAsync(fileName);
                }
            }
        }
    }

    public async Task ExecuteAsync(string title, CancellationToken cancellationToken)
    {
        var tmdbTvResult = await this.tmdb.SearchTvShowAsync(title);

        if (tmdbTvResult.Results.Any())
        {
            var series = await this.tmdb.GetSeries(tmdbTvResult.Results.First().Id.ToString());
            var year = series.FirstAirDate.HasValue ? series.FirstAirDate.Value.Year : 0;

            string folderName = $"{this.fileSystem.CleanPath(title)} ({year})";
            string basePath = this.fileSystem.Path.Combine(this.options.Value.DataRepositoryPath, "series", folderName);

            await RunInternal(series, basePath);
        }
    }
}