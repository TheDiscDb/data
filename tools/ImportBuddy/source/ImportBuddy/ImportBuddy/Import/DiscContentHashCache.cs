using System.Text.Json;
using Fantastic.FileSystem;
using Microsoft.Extensions.Options;
using Spectre.Console;
using StrawberryShake;
using TheDiscDb.Data.GraphQL;
using TheDiscDb.ImportModels;
using TheDiscDb.InputModels;

namespace ImportBuddy;

public record DiscContentHashCacheItem(string ContentHash, string RelativePath, string MediaType, string DiscFormat, string? TmdbId)
{
}

public class DiscContentHashCache
{
    private Dictionary<string, DiscContentHashCacheItem> offlineCache = new Dictionary<string, DiscContentHashCacheItem>();
    private Dictionary<string, DiscContentHashCacheItem> onlineCache = new Dictionary<string, DiscContentHashCacheItem>();
    bool isLoaded = false;
    bool isLoading = false;
    private readonly IOptions<ImportBuddyOptions> options;
    private readonly IFileSystem fileSystem;
    private readonly GetAllDiscContentHashQuery getAllDiscContentHashQuery;

    public DiscContentHashCache(IOptions<ImportBuddyOptions> options, IFileSystem fileSystem, GetAllDiscContentHashQuery getAllDiscContentHashQuery)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.getAllDiscContentHashQuery = getAllDiscContentHashQuery ?? throw new ArgumentNullException(nameof(getAllDiscContentHashQuery));
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (!this.options.Value.Caching.Enabled || isLoaded || isLoading)
        {
            return;
        }

        Interlocked.Exchange(ref isLoading, true);

        if (this.options.Value.Caching.EnableLocalCache)
        {
            await LoadCacheFromDisc(cancellationToken);
        }

        if (this.options.Value.Caching.EnableRemoteCache)
        {
            await LoadFromWeb(cancellationToken);
        }

        Interlocked.Exchange(ref isLoading, false);
        Interlocked.Exchange(ref isLoaded, true);
    }

    public Task<DiscContentHashCacheItem?> GetDiscByContentHash(string contentHash, CancellationToken cancellationToken = default)
    {
        if (!isLoaded)
        {
            // Load in the background and skip the lookup this time
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            InitializeAsync(cancellationToken).ContinueWith(t => 
            {
                if (t.IsFaulted)
                {
                    AnsiConsole.WriteException(t.Exception!);
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return Task.FromResult<DiscContentHashCacheItem?>(null);
        }

        if (this.onlineCache.TryGetValue(contentHash, out var disc))
        {
            return Task.FromResult<DiscContentHashCacheItem?>(disc);
        }

        if (this.offlineCache.TryGetValue(contentHash, out disc))
        {
            return Task.FromResult<DiscContentHashCacheItem?>(disc);
        }

        return Task.FromResult<DiscContentHashCacheItem?>(null);
    }

    private async Task LoadFromWeb(CancellationToken cancellationToken)
    {
        IOperationResult<IGetAllDiscContentHashResult> result = await this.getAllDiscContentHashQuery.ExecuteAsync(after: null, cancellationToken);
        var pageInfo = ProcessPage(result);
        while (pageInfo != null && pageInfo.HasNextPage)
        {
            result = await this.getAllDiscContentHashQuery.ExecuteAsync(after: pageInfo.EndCursor, cancellationToken);
            pageInfo = ProcessPage(result);
        }
    }

    private IGetAllDiscContentHash_MediaItems_PageInfo? ProcessPage(IOperationResult<IGetAllDiscContentHashResult> result)
    {
        if (result.IsSuccessResult())
        {
            var pageInfo = result.Data!.MediaItems!.PageInfo!;
            foreach (var item in result.Data!.MediaItems!.Nodes!)
            {
                foreach (var release in item.Releases)
                {
                    foreach (var disc in release.Discs)
                    {
                        if (!string.IsNullOrEmpty(disc!.ContentHash))
                        {
                            string relativePath = $"{item.Type}\\{item.Title} ({item.Year})\\{release.Slug}\\disc{disc.Index:00}.json";
                            onlineCache[disc.ContentHash] = new DiscContentHashCacheItem(disc.ContentHash, relativePath, item.Type!, disc.Format!, item.Externalids.Tmdb);
                        }
                    }
                }
            }
            return result.Data!.MediaItems!.PageInfo;
        }

        return null;
    }

    private async Task LoadCacheFromDisc(CancellationToken cancellationToken)
    {
        await fileSystem.VisitAsync(this.options.Value.DataRepositoryPath!, async (item, cancellationToken) =>
        {
            if (item.Type == FileItemType.File && item.Path.EndsWith(".json"))
            {
                var fileName = fileSystem.Path.GetFileName(item.Path);
                if (fileName.StartsWith("disc"))
                {
                    var json = await fileSystem.File.ReadAllText(item.Path);
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                    var disc = JsonSerializer.Deserialize<Disc>(json, JsonHelper.JsonOptions);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

                    if (!string.IsNullOrEmpty(disc!.ContentHash))
                    {
                        string relativePath = item.Path.Replace(this.options.Value.DataRepositoryPath!, "");
                        string mediaType = item.Path.Contains("series", StringComparison.OrdinalIgnoreCase) ? "Series" : "Movie";

                        string? tmdbId = null;
                        string metadataPath = fileSystem.Path.Combine(fileSystem.Path.GetDirectoryName(fileSystem.Path.GetDirectoryName(item.Path)), MetadataFile.Filename);
                        if (await fileSystem.File.Exists(metadataPath))
                        {
                            var metadataJson = await fileSystem.File.ReadAllText(metadataPath);
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                            var metadata = JsonSerializer.Deserialize<MetadataFile>(metadataJson, JsonHelper.JsonOptions);
                            tmdbId = metadata?.ExternalIds!.Tmdb;
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                        }

                        offlineCache[disc.ContentHash] = new DiscContentHashCacheItem(disc.ContentHash, relativePath, mediaType, disc.Format!, tmdbId);
                    }
                }
            }
        }, cancellationToken);
    }
}
