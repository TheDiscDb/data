using System.Text.Json;
using Fantastic.FileSystem;
using MakeMkv;
using Spectre.Console;
using TheDiscDb;
using TheDiscDb.Import;
using TheDiscDb.ImportModels;

namespace ImportBuddy;

public class FinalizeTask : IConsoleTask
{
    private readonly IFileSystem fileSystem;

    public FinalizeTask(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public ushort Id => 20;

    public string MenuText => "Finalize";

    public async Task ExecuteAsync(string releaseDirectory, bool recurse, CancellationToken cancellationToken = default)
    {
        if (!await this.fileSystem.Directory.Exists(releaseDirectory))
        {
            AnsiConsole.MarkupLine($"[red]'{releaseDirectory}' does not exist.[/]");
            return;
        }

        if (recurse)
        {
            await this.fileSystem.VisitAsync(releaseDirectory, async (item, ct) =>
            {
                if (item.Type == FileItemType.Directory)
                {
                    string releaseJsonPath = this.fileSystem.Path.Combine(item.Path, ReleaseFile.Filename);
                    if (await this.fileSystem.File.Exists(releaseJsonPath, ct))
                    {
                        AnsiConsole.WriteLine($"Finalizing '{releaseDirectory}'");
                        await FinalizeRelease(item.Path, ct);
                    }
                }
            }, cancellationToken);
        }
        else
        {
            string releaseJsonPath = this.fileSystem.Path.Combine(releaseDirectory, ReleaseFile.Filename);
            if (await this.fileSystem.File.Exists(releaseJsonPath, cancellationToken))
            {
                AnsiConsole.WriteLine($"Finalizing '{releaseDirectory}'");
                await FinalizeRelease(releaseDirectory, cancellationToken);
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]No release file found in '{releaseDirectory}'[/]");
            }
        }
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        string releaseDirectory = AnsiConsole.Ask<string>("Release Folder?");
        if (string.IsNullOrEmpty(releaseDirectory))
        {
            AnsiConsole.MarkupLine("[red]You must specify a release folder.[/]");
            return;
        }

        await ExecuteAsync(releaseDirectory, false, cancellationToken);
    }

    public async Task FinalizeRelease(string releaseDirectory, CancellationToken cancellationToken)
    {
        await foreach (var summaryFile in this.fileSystem.Directory.EnumerateFiles(releaseDirectory, "*-summary.txt", cancellationToken))
        {
            string logFile = summaryFile.Replace("-summary", "");
            if (!await this.fileSystem.File.Exists(logFile, cancellationToken))
            {
                AnsiConsole.MarkupLine($"No companion log file found for '{summaryFile}'");
                continue;
            }

            string discOutputPath = logFile.Replace(".txt", ".json");
            TheDiscDb.InputModels.Disc? disc = new TheDiscDb.InputModels.Disc();
            if (await this.fileSystem.File.Exists(discOutputPath, cancellationToken))
            {
                string discJson = await this.fileSystem.File.ReadAllText(discOutputPath, cancellationToken);
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                disc = JsonSerializer.Deserialize<TheDiscDb.InputModels.Disc>(discJson, JsonHelper.JsonOptions);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            }

            DiscInfo? discInfo = null;
            bool hasRetried = false;

        startOrganize:
            try
            {
                discInfo = LogParser.Organize(logFile);
            }
            catch (IOException)
            {
                // Usually if a log file is locked it is being held onto by java.exe
                if (!hasRetried)
                {
                    hasRetried = true;
                    goto startOrganize;
                }
                AnsiConsole.MarkupLine($"[red bold]Error:[/] Unable to read '{logFile}'. Try killing all running java.exe processes and try again.");
                return;
            }

            string contents = await this.fileSystem.File.ReadAllText(summaryFile, cancellationToken);
            var discFile = SummaryFileParser.ParseSingleDisc(contents);
            discFile.Index = disc?.Index ?? 0;

            if (disc == null)
            {
                disc = new TheDiscDb.InputModels.Disc();
            }

            DiscFileFinalizer.Map(disc, discFile, discInfo);

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            string json = JsonSerializer.Serialize(disc, JsonHelper.JsonOptions);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            await this.fileSystem.File.WriteAllText(discOutputPath, json, cancellationToken);

            await TrySetUpc(discFile, releaseDirectory, cancellationToken);
        }
    }

    private async Task TrySetUpc(DiscFile discFile, string releasePath, CancellationToken cancellationToken)
    {
        foreach (var item in discFile.MainMovies)
        {
            if (!string.IsNullOrEmpty(item.Upc))
            {
                string releaseFile = this.fileSystem.Path.Combine(releasePath, ReleaseFile.Filename);
                if (await this.fileSystem.File.Exists(releaseFile, cancellationToken))
                {
                    string json = await this.fileSystem.File.ReadAllText(releaseFile, cancellationToken);
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                    ReleaseFile? file = JsonSerializer.Deserialize<ReleaseFile>(json, JsonHelper.JsonOptions);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code

                    if (file == null)
                    {
                        file = new ReleaseFile();
                    }

                    file.Upc = item.Upc;
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                    await this.fileSystem.File.WriteAllText(releaseFile, JsonSerializer.Serialize(file, JsonHelper.JsonOptions), cancellationToken);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                }
            }
        }
    }
}

public class BufferedConsoleWriter : IDisposable
{
    private readonly List<string> messages = new List<string>();

    public BufferedConsoleWriter(string context)
    {
        ArgumentException.ThrowIfNullOrEmpty(context);
        Context = context;
    }

    public string Context { get; }

    public void Dispose()
    {
        if (messages.Count > 0)
        {
            AnsiConsole.WriteLine($"Warnings for {Context}:");

            foreach (var warning in messages)
            {
                AnsiConsole.MarkupLine("\t" + warning);
            }
        }
    }

    public void Write(string message)
    {
        messages.Add(message);
    }
}

public class Mapper
{
    public TheDiscDb.InputModels.Title Map(TheDiscDb.LogModels.Title title)
    {
        return new TheDiscDb.InputModels.Title
        {
            DisplaySize = title.DisplaySize,
            Index = title.Index,
            Duration = title.Length,
            SourceFile = title.Playlist,
            SegmentMap = title.SegmentMap,
            Size = title.Size,
            Comment = title.Comment,
            Tracks = title.Tracks.Select(this.Map).ToList()
        };
    }

    public TheDiscDb.InputModels.Track Map(TheDiscDb.LogModels.Track track)
    {
        return new TheDiscDb.InputModels.Track
        {
            AspectRatio = track.AspectRatio,
            AudioType = track.AudioType,
            Index = track.Index,
            Language = track.Language,
            LanguageCode = track.LanguageCode,
            Name = track.Name,
            Resolution = track.Resolution,
            Type = track.Type
        };
    }

    public TheDiscDb.LogModels.Disc Map(DiscInfo item)
    {
        var disc = new TheDiscDb.LogModels.Disc
        {
            Name = item.Name,
            Media = item.Type,
            Language = item.Language
        };

        foreach (var title in item.Titles)
        {
            var t = new TheDiscDb.LogModels.Title
            {
                Index = title.Index,
                DisplaySize = title.DisplaySize,
                SegmentMap = title.SegmentMap,
                Comment = title.Comment,
                Size = title.Size,
                Playlist = title.Playlist,
                Length = title.Length
            };

            foreach (var segment in title.Segments)
            {
                t.Tracks.Add(new TheDiscDb.LogModels.Track
                {
                    AspectRatio = segment.AspectRatio,
                    AudioType = segment.AudioType,
                    Index = segment.Index,
                    Language = segment.Language,
                    LanguageCode = segment.LanguageCode,
                    Name = segment.Name,
                    Resolution = segment.Resolution,
                    Type = segment.Type
                });
            }

            disc.Titles.Add(t);
        }

        return disc;
    }
}