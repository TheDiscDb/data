
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Fantastic.FileSystem;
using Fantastic.TheMovieDb;
using Microsoft.Extensions.Options;
using Spectre.Console;
using TheDiscDb;
using TheDiscDb.Imdb;
using TheDiscDb.Import;
using TheDiscDb.ImportModels;
using TheDiscDb.Tools.MakeMkv;
using static ImportBuddy.ImportTask;

namespace ImportBuddy;

public class ImportTask : IConsoleTask
{
    public const string Name = "Import";
    private readonly ImportMiddlewareManager importManager;
    private readonly TheMovieDbClient tmdb;
    private readonly MakeMkvHelper makeMkv;
    private readonly IOptions<ImportBuddyOptions> options;
    private readonly IFileSystem fileSystem;
    private readonly HttpClient httpClient;
    private readonly IEnumerable<IImportTask> importTasks;
    private readonly DiscContentHashCache discContentCache;
    public static Drive SkipImport = new Drive
    {
        Index = -1,
        Name = "Skip MKV Import",
        Letter = "|"
    };

    public ImportTask(ImportMiddlewareManager importManager, TheMovieDbClient tmdb, MakeMkvHelper makeMkv, IOptions<ImportBuddyOptions> options, IFileSystem fileSystem, HttpClient httpClient, IEnumerable<IImportTask> importTasks, DiscContentHashCache discContentCache)
    {
        this.importManager = importManager ?? throw new ArgumentNullException(nameof(importManager));
        this.tmdb = tmdb ?? throw new ArgumentNullException(nameof(tmdb));
        this.makeMkv = makeMkv ?? throw new ArgumentNullException(nameof(makeMkv));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.importTasks = importTasks ?? throw new ArgumentNullException(nameof(importTasks));
        this.discContentCache = discContentCache ?? throw new ArgumentNullException(nameof(discContentCache));
        if (this.options.Value.DataRepositoryPath == null)
        {
            throw new Exception("DataRepositoryPath is not set in the configuration file");
        }
    }

    public ushort Id => 10;

    public string MenuText => Name;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var data = new ImportData();
        await this.importManager.GetDrive.ProcessAsync(data, cancellationToken);
        await this.importManager.CalculateHash.ProcessAsync(data, cancellationToken);
        await this.importManager.ExistingDiscLookup.ProcessAsync(data, cancellationToken);

        string? title = null;

        if (data.ExistingDiscFound)
        {
            //AnsiConsole.WriteLine($"Warning: This disc is already in the database. {data.ExistingDisc!.RelativePath}");
            bool copyFromExisting = AnsiConsole.Confirm("This disc is already in the database. If this is a new release would you like to copy it?", defaultValue: false);
            if (copyFromExisting)
            {
                // Get the number of the target disc

                // Get the destination release folder

                //await this.importManager.ImportFromExisting.ProcessAsync(data, cancellationToken);

                //data.DiscFormat = data.ExistingDisc!.DiscFormat;
                //data.ItemType = ImportData.GetItemType(data.ExistingDisc.MediaType);
                //title = data.ExistingDisc.TmdbId;
                //await this.CopyExistingDisc(data, releaseFolder, cancellationToken);
                //return;
            }
        }

        if (string.IsNullOrEmpty(title))
        {
            title = AnsiConsole.Ask<string>("TMDB ID:");
        }

        if (!data.ItemType.HasValue)
        {
            var itemPrompt = new SelectionPrompt<string>();
            itemPrompt.AddChoice("Movie");
            itemPrompt.AddChoice("Series");

            data.ItemType = ImportData.GetItemType(AnsiConsole.Prompt(itemPrompt));
        }

        ImportItem? importItem = null;

        foreach (var task in importTasks)
        {
            if (task.CanHandle(title))
            {
                var result = await task.GetImportItem(title, data.ItemType ?? ImportItemType.Movie, cancellationToken);
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

        string? posterUrl = importItem.GetPosterUrl();
        int year = importItem.TryGetYear();

        if (string.IsNullOrEmpty(data.DiscFormat))
        {
            data.DiscFormat = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Disc Format")
                .AddChoices("Blu-Ray", "UHD", "DVD"));
        }

        MetadataFile? metadata = ImportHelper.BuildMetadata(importItem.ImdbTitle, importItem.GetTmdbItemToSerialize() as Fantastic.TheMovieDb.Models.Movie, importItem.GetTmdbItemToSerialize() as Fantastic.TheMovieDb.Models.Series, year, data.ItemType ?? ImportItemType.Movie);

        if (metadata != null && metadata.Title == null)
        {
            AnsiConsole.WriteLine("Could not determine title for metadata");
            return;
        }

        string folderName = $"{this.fileSystem.CleanPath(metadata!.Title!)} ({year})";
        string subFolderName = "movie";
        if (data.ItemType == ImportItemType.Series)
        {
            subFolderName = "series";
        }

        string basePath = this.fileSystem.Path.Combine(this.options.Value.DataRepositoryPath!, subFolderName, folderName);
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
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            await this.fileSystem.File.WriteAllText(tmdbPath, JsonSerializer.Serialize(importItem.GetTmdbItemToSerialize(), JsonHelper.JsonOptions));
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        }

        string imdbPath = this.fileSystem.Path.Combine(basePath, "imdb.json");
        if (!await this.fileSystem.File.Exists(imdbPath) && importItem.ImdbTitle != null)
        {
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            await this.fileSystem.File.WriteAllText(imdbPath, JsonSerializer.Serialize(importItem.ImdbTitle, JsonHelper.JsonOptions));
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        }

        string metadataPath = this.fileSystem.Path.Combine(basePath, MetadataFile.Filename);
        if (!await this.fileSystem.File.Exists(metadataPath))
        {
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            await this.fileSystem.File.WriteAllText(metadataPath, JsonSerializer.Serialize(metadata, JsonHelper.JsonOptions));
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        }


        // Construct a default slug from the title year and disc format.
        //  If this slug already exists, it will be detected and handled at the "Create Release" conditional.
        string defaultReleaseSlug = ImportHelper.CreateSlug(data.DiscFormat, year);

        var releaseFolders = await this.fileSystem.Directory.GetDirectories(basePath, cancellationToken);
        bool hasReleases = releaseFolders.Any();
        string releaseSlug = "release";
        string releaseFolder = this.fileSystem.Path.Combine(basePath, releaseSlug);

        if (hasReleases)
        {
            bool addNewRelease = AnsiConsole.Confirm("Add New release?");
            if (addNewRelease)
            {
                // Prompt for the user to input a release slug, including the default release slug if it exists.
                releaseSlug = string.IsNullOrEmpty(defaultReleaseSlug) ? AnsiConsole.Ask<string>("New Release Slug:") : AnsiConsole.Ask<string>("New Release Slug", defaultReleaseSlug);
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
            // Prompt for the user to input a release slug, including the default release slug if it exists.
            releaseSlug = string.IsNullOrEmpty(defaultReleaseSlug) ? AnsiConsole.Ask<string>("New Release Slug:") : AnsiConsole.Ask<string>("New Release Slug", defaultReleaseSlug);
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
                    SortTitle = $"{releaseYear} {ImportHelper.GetSortTitle(releaseName)}",
                    Slug = releaseSlug,
                    Upc = upc,
                    Locale = "en-us",
                    Year = releaseYear,
                    RegionCode = "1",
                    Asin = asin,
                    ReleaseDate = releaseDate,
                    DateAdded = DateTimeOffset.UtcNow.Date
                };

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                string json = JsonSerializer.Serialize(release, JsonHelper.JsonOptions);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                await this.fileSystem.File.WriteAllText(releaseFile, json);
            }
        }

        var discName = await this.fileSystem.GetDiscName(releaseFolder);
        string discTitle = AnsiConsole.Ask<string>("Disc Name:");
        string discSlug = AnsiConsole.Ask<string>("Disc Slug:");

        string resolution = "1080p";
        if (data.DiscFormat.Equals("UHD", StringComparison.OrdinalIgnoreCase))
        {
            resolution = "2160p";
        }
        else if (data.DiscFormat.Equals("DVD", StringComparison.OrdinalIgnoreCase))
        {
            resolution = "720p";
        }

        string formattedTitle = $"{folderName} [{resolution}].mkv";

        string makeMkvLogPath = this.fileSystem.Path.Combine(releaseFolder, $"{discName.Name}.txt");
        if (!await this.fileSystem.File.Exists(makeMkvLogPath))
        {
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

            string discJsonFilePath = this.fileSystem.Path.Combine(releaseFolder, $"{discName.Name}.json");
            if (!await this.fileSystem.File.Exists(discJsonFilePath))
            {
                var discJsonFile = new DiscFile
                {
                    Index = discName.Index,
                    Slug = discSlug,
                    Name = discTitle,
                    Format = data.DiscFormat,
                    ContentHash = data.HashInfo?.Hash
                };

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                await this.fileSystem.File.WriteAllText(discJsonFilePath, JsonSerializer.Serialize(discJsonFile, JsonHelper.JsonOptions));
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            }

            if (AnsiConsole.Confirm("Start MakeMKV import?", defaultValue: true))
            {
                AnsiConsole.WriteLine("Importing MakeMkv logs...");
                try
                {
                    await this.makeMkv.WriteLogs(data.Drive!.Index, makeMkvLogPath);

                    if (data.HashInfo != null)
                    {
                        await DiskContentHash.TryAppendHashInfo(this.fileSystem, makeMkvLogPath, data.HashInfo, cancellationToken);
                    }
                }
                catch (CleanLogFileException e)
                {
                    AnsiConsole.WriteLine(e.Message);
                }
            }
        }
    }

    private async Task CopyExistingDisc(ImportData data, string releaseFolder, CancellationToken cancellationToken)
    {
        var discName = await this.fileSystem.GetDiscName(releaseFolder);
        string discTitle = AnsiConsole.Ask<string>("Disc Name:");
        string discSlug = AnsiConsole.Ask<string>("Disc Slug:");

        string inputDirectory = this.fileSystem.Path.GetDirectoryName(this.fileSystem.Path.Combine(this.options.Value.DataRepositoryPath!, data.ExistingDisc!.RelativePath));

        // copy make mkv log file
        string sourceMkvLogPath = this.fileSystem.Path.Combine(inputDirectory, $"{discName.Name}.txt");
        string destinationMkvLogPath = this.fileSystem.Path.Combine(releaseFolder, $"{discName.Name}.txt");
        await this.fileSystem.File.Copy(sourceMkvLogPath, destinationMkvLogPath, overwrite: true);

        // copy summary file
        string sourceSummaryPath = this.fileSystem.Path.Combine(inputDirectory, $"{discName.Name}-summary.txt");
        string destinationSummaryPath = this.fileSystem.Path.Combine(releaseFolder, $"{discName.Name}-summary.txt");
        await this.fileSystem.File.Copy(sourceSummaryPath, destinationSummaryPath, overwrite: true);

        // copy disc json file
        string sourceDiscJsonPath = this.fileSystem.Path.Combine(inputDirectory, $"{discName.Name}.json");
        string destinationDiscJsonPath = this.fileSystem.Path.Combine(releaseFolder, $"{discName.Name}.json");
        await this.fileSystem.File.Copy(sourceDiscJsonPath, destinationDiscJsonPath, overwrite: true);

        // update disc json file
        var discFileJson = await this.fileSystem.File.ReadAllText(destinationDiscJsonPath);
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
        var discFile = JsonSerializer.Deserialize<DiscFile>(discFileJson, JsonHelper.JsonOptions);
        discFile!.Name = discTitle;
        discFile!.Slug = discSlug;
        discFileJson = JsonSerializer.Serialize(discFile, JsonHelper.JsonOptions);
        await this.fileSystem.File.WriteAllText(destinationDiscJsonPath, discFileJson);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
    }
}
