using Fantastic.FileSystem;
using Spectre.Console;

namespace ImportBuddy;

public class CalculateDiscContentHashMiddleware : ImportMiddleware
{
    private readonly IFileSystem fileSystem;

    public CalculateDiscContentHashMiddleware(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public override async Task ProcessAsync(ImportData data, CancellationToken cancellationToken = default)
    {
        if (data.Drive?.Letter == null)
        {
            AnsiConsole.WriteLine("The selected drive does not have a Letter configured. No Hash will be calculated.");
        }
        else
        {
            data.HashInfo = await this.fileSystem.HashMediaDisc(data.Drive.Letter[0]);
            if (data.HashInfo == null)
            {
                AnsiConsole.WriteLine("Warning: Could not calculate disc hash");
            }
        }
    }
}
