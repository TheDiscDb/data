namespace ImportBuddy;

public interface IImportTask
{
    bool CanHandle(string title, string itemType);
    Task<ImportItem?> GetImportItem(string title, string itemType, CancellationToken cancellationToken = default);
}
