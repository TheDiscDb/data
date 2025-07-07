using System.Text.Json;
using Fantastic.FileSystem;
using MakeMkv;
using Microsoft.Extensions.Options;
using Spectre.Console;
using TheDiscDb.Core.DiscHash;
using TheDiscDb.ImportModels;
using TheDiscDb.Tools.MakeMkv;

namespace ImportBuddy;

public class DiscContentHashTask : IConsoleTask
{
    public ushort Id => 50;
    public string MenuText => "Calculate Disc Hash";

    private readonly IFileSystem fileSystem;
    private readonly MakeMkvHelper makeMkv;
    private readonly IOptions<ImportBuddyOptions> options;

    public DiscContentHashTask(IFileSystem fileSystem, MakeMkvHelper makeMkv, IOptions<ImportBuddyOptions> options)
    {
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.makeMkv = makeMkv ?? throw new ArgumentNullException(nameof(makeMkv));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var driveChoices = new SelectionPrompt<Drive>();
        driveChoices.Converter = d => $"{d.Index}: {d.Letter}: {d.Name}";
        foreach (var drive in this.makeMkv.Drives)
        {
            driveChoices.AddChoice(drive);
        }

        var driveChoice = AnsiConsole.Prompt(driveChoices);

        string discFile = AnsiConsole.Prompt(new TextPrompt<string>("Disc File:"));

        await SingleDiskMode(driveChoice, discFile, cancellationToken);
    }

    private async Task<bool> HandleSingleDisc(DiscHashInfo? hashInfo, string? file, TheDiscDb.InputModels.Disc? disc, CancellationToken cancellationToken)
    {
        if (disc == null || hashInfo == null || string.IsNullOrEmpty(file))
        {
            return false;
        }

        // Only rewrite the file if the hash changed
        if (disc.ContentHash != hashInfo.Hash)
        {
            disc.ContentHash = hashInfo.Hash;

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            string json = JsonSerializer.Serialize(disc, JsonHelper.JsonOptions);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            await this.fileSystem.File.WriteAllText(file, json, cancellationToken);
        }

        string logFile = Path.ChangeExtension(file, ".txt");

        await TryAppendHashInfo(this.fileSystem, logFile, hashInfo, cancellationToken);
        return true;
    }

    public static async Task TryAppendHashInfo(IFileSystem fileSystem, string logFile, DiscHashInfo hashInfo, CancellationToken cancellationToken = default)
    {
        if (!await fileSystem.File.Exists(logFile))
        {
            return;
        }

        var lines = File.ReadAllLines(logFile);
        var result = new List<string>();
        foreach (var line in lines)
        {
            if (!line.StartsWith(HashInfoLogLine.LinePrefix, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(line);
            }
        }

        foreach (var info in hashInfo.Files)
        {
            var logLine = new HashInfoLogLine
            {
                Index = info.Index,
                Name = info.Name,
                CreationTime = info.CreationTime,
                Size = info.Size
            };

            result.Add(logLine.ToString());
        }

        await fileSystem.File.WriteAllLines(logFile, result, cancellationToken);
    }

    public async Task SingleDiskMode(Drive driveChoice, string discFile, CancellationToken cancellationToken = default)
    {
        if (driveChoice?.Letter == null)
        {
            return;
        }

        var drive = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom && d.RootDirectory.Name.StartsWith(driveChoice.Letter)).FirstOrDefault();
        if (drive == null)
        {
            AnsiConsole.MarkupLine($"[red]Drive '{driveChoice.Letter}' not found.[/]");
            return;
        }

        discFile = discFile.Replace("\"", string.Empty);
        List<string> discFiles = [discFile];
        if (discFile.IndexOf('|') > -1)
        {
            discFiles = discFile.Split('|').ToList();
        }

        foreach (var file in discFiles)
        {
            if (!await this.fileSystem.File.Exists(file))
            {
                AnsiConsole.MarkupLine($"[red]'{file}' does not exist.[/]");
                break;
            }
        }

        var stopWatch = new System.Diagnostics.Stopwatch();
        stopWatch.Start();
        var hashInfo = await this.fileSystem.HashMediaDisc(driveChoice.Letter[0]);
        stopWatch.Stop();

        AnsiConsole.WriteLine($"Content Hash: {hashInfo?.Hash} ({stopWatch.Elapsed.TotalSeconds}s)");

        foreach (var file in discFiles)
        {
            string json = await this.fileSystem.File.ReadAllText(file, cancellationToken);
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            var disc = JsonSerializer.Deserialize<TheDiscDb.InputModels.Disc>(json, JsonHelper.JsonOptions);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            bool success = await HandleSingleDisc(hashInfo, file, disc, cancellationToken);
            // TODO: Handle non success
        }
    }
}
