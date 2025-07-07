namespace ImportBuddy;

public class ImportMiddlewareManager
{
    public ImportMiddlewareManager(GetDriveImportMiddleware getDrive, CalculateDiscContentHashMiddleware calculateHash, ExistingDiscLookupMiddleware existingDiscLookup, ImportFromExistingMiddleware importFromExisting)
    {
        GetDrive = getDrive;
        CalculateHash = calculateHash;
        ExistingDiscLookup = existingDiscLookup;
        ImportFromExisting = importFromExisting;
    }

    public GetDriveImportMiddleware GetDrive { get; }
    public CalculateDiscContentHashMiddleware CalculateHash { get; }
    public ExistingDiscLookupMiddleware ExistingDiscLookup { get; }
    public ImportFromExistingMiddleware ImportFromExisting { get; }
}
