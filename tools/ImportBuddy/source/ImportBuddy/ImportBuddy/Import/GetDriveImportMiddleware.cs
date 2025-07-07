using Spectre.Console;
using TheDiscDb.Tools.MakeMkv;

namespace ImportBuddy;

public class GetDriveImportMiddleware : ImportMiddleware
{
    private readonly MakeMkvHelper makeMkv;

    public GetDriveImportMiddleware(MakeMkvHelper makeMkv)
    {
        this.makeMkv = makeMkv ?? throw new ArgumentNullException(nameof(makeMkv));
    }

    public override Task ProcessAsync(ImportData data, CancellationToken cancellationToken = default)
    {
        if (this.makeMkv.Drives.Count == 1)
        {
            data.Drive = this.makeMkv.Drives.First();
        }
        else
        {
            var driveChoices = new SelectionPrompt<Drive>();
            driveChoices.Converter = d => $"{d.Index}: {d.Letter}: {d.Name}";
            foreach (var drive in this.makeMkv.Drives)
            {
                driveChoices.AddChoice(drive);
            }

            data.Drive = AnsiConsole.Prompt(driveChoices);
        }

        return Task.CompletedTask;
    }
}
