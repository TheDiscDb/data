namespace ImportBuddy;

public abstract class ImportMiddleware
{
    public abstract Task ProcessAsync(ImportData data, CancellationToken cancellationToken = default);
}
