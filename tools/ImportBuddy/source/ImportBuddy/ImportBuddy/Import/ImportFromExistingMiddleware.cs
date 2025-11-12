using Fantastic.FileSystem;
using Microsoft.Extensions.Options;
using TheDiscDb.Import;

namespace ImportBuddy;

public class ImportFromExistingMiddleware : ImportMiddleware
{
    private readonly IFileSystem fileSystem;
    private readonly IOptions<ImportBuddyOptions> options;
    public ImportFromExistingMiddleware(IFileSystem fileSystem, IOptions<ImportBuddyOptions> options)
    {
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public override async Task ProcessAsync(ImportData data, CancellationToken cancellationToken = default)
    {
        if (!data.ExistingDiscFound)
        {
            return;
        }

        string inputDirectory = this.fileSystem.Path.GetDirectoryName(this.fileSystem.Path.Combine(this.options.Value.DataRepositoryPath!, data.ExistingDisc!.RelativePath));

        data.ImportItem = await RecentItemImportTask.GetImportItem(this.fileSystem, inputDirectory, data.ItemType ?? ImportItemType.Movie, cancellationToken);
    }
}
