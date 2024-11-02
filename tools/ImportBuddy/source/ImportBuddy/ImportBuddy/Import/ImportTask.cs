
using System.Text;
using System.Text.Json;
using Fantastic.FileSystem;
using Fantastic.TheMovieDb;
using TheDiscDb.Imdb;
using Microsoft.Extensions.Options;
using Spectre.Console;
using TheDiscDb;
using TheDiscDb.ImportModels;
using TheDiscDb.Tools.MakeMkv;

namespace ImportBuddy;

public class ImportTask : IConsoleTask
{
    public const string Name = "Import";

    private readonly TheMovieDbClient tmdb;
    private readonly MakeMkvHelper makeMkv;
    private readonly IOptions<ImportBuddyOptions> options;
    private readonly IFileSystem fileSystem;
    private readonly HttpClient httpClient;
    private readonly IEnumerable<IImportTask> importTasks;

    public static Drive SkipImport = new Drive
    {
        Index = -1,
        Name = "Skip MKV Import",
        Letter = "|"
    };

    public ImportTask(TheMovieDbClient tmdb, MakeMkvHelper makeMkv, IOptions<ImportBuddyOptions> options, IFileSystem fileSystem, HttpClient httpClient, IEnumerable<IImportTask> importTasks)
    {
        this.tmdb = tmdb ?? throw new ArgumentNullException(nameof(tmdb));
        this.makeMkv = makeMkv ?? throw new ArgumentNullException(nameof(makeMkv));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.importTasks = importTasks ?? throw new ArgumentNullException(nameof(importTasks));
    }

    public ushort Id => 10;

    public string MenuText => Name;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        string title = AnsiConsole.Ask<string>("TMDB ID:");

        var itemPrompt = new SelectionPrompt<string>();
        itemPrompt.AddChoice("Movie");
        itemPrompt.AddChoice("Series");

        string itemType = AnsiConsole.Prompt(itemPrompt);

        ImportItem importItem = null;

        foreach (var task in importTasks)
        {
            if (task.CanHandle(title, itemType))
            {
                var result = await task.GetImportItem(title, itemType, cancellationToken);
                if (result != null)
                {
                    importItem = result;
                    break;
                }
            }
        }

        if (importItem == null)
        {
            AnsiConsole.WriteLine("Could not find any valid import item");
            return;
        }

        string posterUrl = importItem.GetPosterUrl();
        int year = importItem.TryGetYear();

        var format = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Disc Format")
            .AddChoices("Blu-Ray", "UHD", "DVD"));

        var metadata = BuildMetadata(importItem.ImdbTitle, importItem.GetTmdbItemToSerialize() as Fantastic.TheMovieDb.Models.Movie, importItem.GetTmdbItemToSerialize() as Fantastic.TheMovieDb.Models.Series, year, itemType);

        string folderName = $"{this.fileSystem.CleanPath(metadata.Title)} ({year})";
        string subFolderName = "movie";
        if (itemType.Equals("Series", StringComparison.OrdinalIgnoreCase))
        {
            subFolderName = "series";
        }

        string basePath = this.fileSystem.Path.Combine(this.options.Value.DataRepositoryPath, subFolderName, folderName);
        AnsiConsole.MarkupLine($"Importing into [bold]{basePath}[/]");

        if (!(await this.fileSystem.Directory.Exists(basePath)))
        {
            await this.fileSystem.Directory.CreateDirectory(basePath);
        }

        if (subFolderName == "series")
        {
            var getSeriesFilenameTask = new GetSeriesFilenamesTask(this.options, this.fileSystem, this.tmdb);
            await getSeriesFilenameTask.RunInternal(importItem.GetTmdbItemToSerialize() as Fantastic.TheMovieDb.Models.Series, basePath);
        }

        string posterPath = this.fileSystem.Path.Combine(basePath, "cover.jpg");
        if (!await this.fileSystem.File.Exists(posterPath) && !string.IsNullOrEmpty(posterUrl))
        {
            AnsiConsole.MarkupLine("Downloading poster...");
            await this.httpClient.Download(this.fileSystem, posterUrl, posterPath);
        }

        string tmdbPath = this.fileSystem.Path.Combine(basePath, "tmdb.json");
        if (!await this.fileSystem.File.Exists(tmdbPath) && importItem.GetTmdbItemToSerialize() != null)
        {
            await this.fileSystem.File.WriteAllText(tmdbPath, JsonSerializer.Serialize(importItem.GetTmdbItemToSerialize(), JsonHelper.JsonOptions));
        }

        string imdbPath = this.fileSystem.Path.Combine(basePath, "imdb.json");
        if (!await this.fileSystem.File.Exists(imdbPath) && importItem.ImdbTitle != null)
        {
            await this.fileSystem.File.WriteAllText(imdbPath, JsonSerializer.Serialize(importItem.ImdbTitle, JsonHelper.JsonOptions));
        }

        string metadataPath = this.fileSystem.Path.Combine(basePath, MetadataFile.Filename);
        if (!await this.fileSystem.File.Exists(metadataPath))
        {
            await this.fileSystem.File.WriteAllText(metadataPath, JsonSerializer.Serialize(metadata, JsonHelper.JsonOptions));
        }

        var releaseFolders = await this.fileSystem.Directory.GetDirectories(basePath, cancellationToken);
        bool hasReleases = releaseFolders.Any();
        string releaseSlug = "release";
        string releaseFolder = this.fileSystem.Path.Combine(basePath, releaseSlug);

        if (hasReleases)
        {
            //var releasePrompt = new SelectionPrompt<string>();
            //itemPrompt.AddChoice("Add New Release");
            //itemPrompt.AddChoice("Add to Existing Release");
            //string itemType = AnsiConsole.Prompt(releasePrompt);

            bool addNewRelease = AnsiConsole.Confirm("Add New release?");
            if (addNewRelease)
            {
                releaseSlug = AnsiConsole.Ask<string>("New Release Slug:");
                releaseFolder = this.fileSystem.Path.Combine(basePath, releaseSlug);
            }
            else
            {
                if (releaseFolders.Count() > 1)
                {
                    var releasePrompt = new SelectionPrompt<string>();
                    foreach (var releaseDir in releaseFolders)
                    {
                        releasePrompt.AddChoice(this.fileSystem.Path.GetFileName(releaseDir));
                    }

                    string existingReleaseName = AnsiConsole.Prompt(releasePrompt);
                    foreach (var releaseDir in releaseFolders)
                    {
                        string slug = this.fileSystem.Path.GetFileName(releaseDir);
                        if (slug == existingReleaseName)
                        {
                            releaseFolder = releaseDir;
                            releaseSlug = slug;
                        }
                    }
                }
                else
                {
                    releaseFolder = releaseFolders.First();
                    releaseSlug = this.fileSystem.Path.GetFileName(releaseFolder);
                }
            }
        }
        else
        {
            releaseSlug = AnsiConsole.Ask<string>("New Release Slug:");
            releaseFolder = this.fileSystem.Path.Combine(basePath, releaseSlug);
        }

        // Create Release
        if (!await this.fileSystem.Directory.Exists(releaseFolder))
        {
            await this.fileSystem.Directory.CreateDirectory(releaseFolder);

            string upc = AnsiConsole.Prompt(new TextPrompt<string>("UPC (optional):").AllowEmpty());
            string asin = AnsiConsole.Prompt(new TextPrompt<string>("ASIN (optional):").AllowEmpty());
            string releaseDateString = AnsiConsole.Prompt(new TextPrompt<string>("Release Date:").AllowEmpty());
            string frontCoverUrl = AnsiConsole.Prompt(new TextPrompt<string>("Front Cover:").AllowEmpty());
            string backCoverUrl = AnsiConsole.Prompt(new TextPrompt<string>("Back Cover:").AllowEmpty());
            string releaseName = AnsiConsole.Ask<string>("New Release Name:");

            if (!string.IsNullOrEmpty(frontCoverUrl))
            {
                string frontCoverPath = this.fileSystem.Path.Combine(releaseFolder, "front.jpg");
                await this.httpClient.Download(this.fileSystem, frontCoverUrl, frontCoverPath);

                //TODO: Update image url in the release file (and upload to blob storage)?
            }

            if (!string.IsNullOrEmpty(backCoverUrl))
            {
                string backCoverPath = this.fileSystem.Path.Combine(releaseFolder, "back.jpg");
                await this.httpClient.Download(this.fileSystem, backCoverUrl, backCoverPath);
            }

            DateTimeOffset releaseDate = default;
            int releaseYear = year;
            if (!string.IsNullOrEmpty(releaseDateString))
            {
                // Format: October 25, 2022
                DateTimeOffset.TryParse(releaseDateString, out releaseDate);
                releaseYear = releaseDate.Year;
            }

            string releaseFile = this.fileSystem.Path.Combine(releaseFolder, ReleaseFile.Filename);
            if (!await this.fileSystem.Directory.Exists(releaseFile))
            {
                var release = new ReleaseFile
                {
                    Title = releaseName,
                    SortTitle = $"{releaseYear} {GetSortTitle(releaseName)}",
                    Slug = releaseSlug,
                    Upc = upc,
                    Locale = "en-us",
                    Year = releaseYear,
                    RegionCode = "1",
                    Asin = asin,
                    ReleaseDate = releaseDate,
                    DateAdded = DateTimeOffset.UtcNow.Date
                };

                string json = JsonSerializer.Serialize(release, JsonHelper.JsonOptions);
                await this.fileSystem.File.WriteAllText(releaseFile, json);
            }
        }

        var discName = await this.GetDiscName(releaseFolder);
        string discTitle = AnsiConsole.Ask<string>("Disc Name:");
        string discSlug = AnsiConsole.Ask<string>("Disc Slug:");

        string resolution = "1080p";
        if (format.Equals("UHD", StringComparison.OrdinalIgnoreCase))
        {
            resolution = "2160p";
        }
        else if (format.Equals("DVD", StringComparison.OrdinalIgnoreCase))
        {
            resolution = "720p";
        }

        string formattedTitle = $"{folderName} [{resolution}].mkv";
        string contentHash = string.Empty;

        string makeMkvLogPath = this.fileSystem.Path.Combine(releaseFolder, $"{discName.Name}.txt");
        if (!await this.fileSystem.File.Exists(makeMkvLogPath))
        {
            var driveChoices = new SelectionPrompt<Drive>();
            driveChoices.Converter = d => $"{d.Index}: {d.Letter}: {d.Name}";
            foreach (var drive in this.makeMkv.Drives)
            {
                driveChoices.AddChoice(drive);
            }

            driveChoices.AddChoice(SkipImport);

            var driveChoice = AnsiConsole.Prompt(driveChoices);

            var summaryContents = new StringBuilder();
            summaryContents.AppendLine($"Name: {metadata.Title}");
            summaryContents.AppendLine("Type: MainMovie");
            summaryContents.AppendLine($"Year: {metadata.Year}");
            summaryContents.AppendLine($"File name: {formattedTitle}");

            string summaryPath = this.fileSystem.Path.Combine(releaseFolder, $"{discName.Name}-summary.txt");
            if (!await this.fileSystem.File.Exists(summaryPath))
            {
                await this.fileSystem.File.WriteAllText(summaryPath, summaryContents.ToString());
            }

            if (driveChoice.Index != SkipImport.Index)
            {
                AnsiConsole.WriteLine("Importing MakeMkv logs...");
                try
                {
                    await this.makeMkv.WriteLogs(driveChoice.Index, makeMkvLogPath);
                }
                catch (CleanLogFileException e)
                {
                    AnsiConsole.WriteLine(e.Message);
                }

                AnsiConsole.WriteLine("Calculating disk content hash...");
                var hashInfo = await this.fileSystem.HashMediaDisc(driveChoice.Letter[0]);
                contentHash = hashInfo.Hash;
                await DiskContentHash.TryAppendHashInfo(this.fileSystem, makeMkvLogPath, hashInfo, cancellationToken);
            }
        }

        string discJsonFilePath = this.fileSystem.Path.Combine(releaseFolder, $"{discName.Name}.json");
        if (!await this.fileSystem.File.Exists(discJsonFilePath))
        {
            var discJsonFile = new DiscFile
            {
                Index = discName.Index,
                Slug = discSlug,
                Name = discTitle,
                Format = format,
                ContentHash = contentHash
            };

            await this.fileSystem.File.WriteAllText(discJsonFilePath, JsonSerializer.Serialize(discJsonFile, JsonHelper.JsonOptions));
        }
    }

    private MetadataFile BuildMetadata(TitleData imdbTitle, Fantastic.TheMovieDb.Models.Movie movie, Fantastic.TheMovieDb.Models.Series series, int year, string type)
    {
        var metadata = new MetadataFile
        {
            Year = year,
            Type = type,
            DateAdded = DateTimeOffset.UtcNow.Date
        };

        if (imdbTitle != null && string.IsNullOrEmpty(imdbTitle.ErrorMessage))
        {
            metadata.Title = imdbTitle.Title;
            metadata.FullTitle = imdbTitle.FullTitle;
            metadata.Slug = this.CreateSlug(imdbTitle.Title, year);
        }
        else if (movie != null)
        {
            metadata.Title = movie.Title;
            metadata.FullTitle = movie.OriginalTitle;
            metadata.Slug = this.CreateSlug(movie.Title, year);
            if (movie.ReleaseDate.HasValue)
            {
                metadata.ReleaseDate = movie.ReleaseDate.Value;
            }
        }
        else if (series != null)
        {
            metadata.Title = series.Name;
            metadata.SortTitle = GetSortTitle(series.Name);
            metadata.SortTitle = GetSortTitle(metadata.Title);
            metadata.FullTitle = series.OriginalName;
            metadata.Slug = this.CreateSlug(series.Name, year);
            if (series.FirstAirDate.HasValue)
            {
                metadata.ReleaseDate = series.FirstAirDate.Value;
            }
        }

        metadata.ExternalIds.Imdb = imdbTitle?.Id;

        if (imdbTitle != null && string.IsNullOrEmpty(imdbTitle.ErrorMessage))
        {
            metadata.Plot = imdbTitle.Plot;
            metadata.Directors = imdbTitle.Directors;
            metadata.Stars = imdbTitle.Stars;
            metadata.Writers = imdbTitle.Writers;
            metadata.Genres = imdbTitle.Genres;
            metadata.Runtime = imdbTitle.RuntimeStr;
            metadata.ContentRating = imdbTitle.ContentRating;
            metadata.Tagline = imdbTitle.Tagline;
            if (metadata.ReleaseDate == default(DateTimeOffset) && !string.IsNullOrEmpty(imdbTitle.ReleaseDate))
            {
                metadata.ReleaseDate = DateTimeOffset.Parse(imdbTitle.ReleaseDate + "T00:00:00+00:00");
            }

            if (Int32.TryParse(imdbTitle.RuntimeMins, out int minutes))
            {
                metadata.RuntimeMinutes = minutes;
            }
        }

        if (movie != null)
        {
            metadata.ExternalIds.Tmdb = movie.Id.ToString();

            if (string.IsNullOrEmpty(metadata.Plot))
            {
                metadata.Plot = movie.Overview;
            }

            if (string.IsNullOrEmpty(metadata.Tagline))
            {
                metadata.Tagline = movie.Tagline;
            }
        }
        else if (series != null)
        {
            metadata.ExternalIds.Tmdb = series.Id.ToString();

            if (string.IsNullOrEmpty(metadata.Plot))
            {
                metadata.Plot = series.Overview;
            }
        }

        if (metadata.Title.StartsWith("the", StringComparison.OrdinalIgnoreCase))
        {
            metadata.SortTitle = metadata.Title.Substring(4, metadata.Title.Length - 4).Trim() + ", The";
        }
        else
        {
            metadata.SortTitle = metadata.Title;
        }

        return metadata;
    }

    private string GetSortTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return title;
        }

        if (title.StartsWith("the", StringComparison.OrdinalIgnoreCase))
        {
            return title.Substring(4, title.Length - 4).Trim() + ", The";
        }
        else
        {
            return title;
        }
    }

    private string CreateSlug(string name, int year)
    {
        if (year != default(int))
        {
            return string.Format("{0}-{1}", name.Slugify(), year);
        }

        return name.Slugify();
    }

    internal struct DiscName
    {
        public string Name;
        public int Index;
    }

    private async Task<DiscName> GetDiscName(string path)
    {
        var files = await this.fileSystem.Directory.GetFiles(path, "*disc*");
        var name = new DiscName
        {
            Name = "disc01",
            Index = 1
        };

        for (int i = 1; i < 100; i++)
        {
            name.Name = string.Format("disc{0:00}", i);
            name.Index = i;

            if (files.Any(f => f.Contains(name.Name)))
            {
                continue;
            }

            break;
        }

        return name;
    }
}
