using Spectre.Console;

namespace ImportBuddy;

public class ExistingDiscLookupMiddleware : ImportMiddleware
{
    private readonly DiscContentHashCache cache;
    public ExistingDiscLookupMiddleware(DiscContentHashCache cache)
    {
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public override async Task ProcessAsync(ImportData data, CancellationToken cancellationToken = default)
    {
        if (data.HashInfo == null)
        {
            AnsiConsole.WriteLine("No HashInfo available to lookup existing disc");
            return;
        }

        data.ExistingDisc = await this.cache.GetDiscByContentHash(data.HashInfo!.Hash!, cancellationToken);
    }
}
