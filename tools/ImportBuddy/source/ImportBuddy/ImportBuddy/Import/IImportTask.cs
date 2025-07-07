namespace ImportBuddy;

public interface IImportTask
{
    bool CanHandle(string title);
    Task<ImportItem?> GetImportItem(string title, ImportItemType itemType, CancellationToken cancellationToken = default);
}
