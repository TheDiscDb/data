using Fantastic.FileSystem;
using MakeMkv;
using Spectre.Console;
using TheDiscDb.Core.DiscHash;

namespace ImportBuddy;

public static class DiskContentHash
{
    public static async Task<DiscHashInfo> HashLogFile(this IFileSystem fileSystem, string logFile)
    {
        var info = new DiscHashInfo();
        var lines = await fileSystem.File.ReadAllLines(logFile);
        foreach (var line in lines)
        {
            if (line.StartsWith(HashInfoLogLine.LinePrefix, StringComparison.OrdinalIgnoreCase))
            {
                var hashInfo = HashInfoLogLine.Parse(line);
                info.Files.Add(new FileHashInfo
                {
                    Index = hashInfo.Index,
                    Name = hashInfo.Name,
                    CreationTime = hashInfo.CreationTime,
                    Size = hashInfo.Size
                });
            }
        }

        info.Hash = info.Files.CalculateHash();

        return info;
    }

    public static async Task<DiscHashInfo?> HashMediaDisc(this IFileSystem fileSystem, char driveLetter)
    {
        string bluRayPath = $@"{driveLetter}:\BDMV\STREAM";
        string dvdPath = $@"{driveLetter}:\VIDEO_TS";

        string? path = null;
        string pattern = "*";

        if (await fileSystem.Directory.Exists(bluRayPath))
        {
            path = bluRayPath;
            pattern = "*.m2ts";
        }
        else if (await fileSystem.Directory.Exists(dvdPath))
        {
            path = dvdPath;
        }

        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        var info = new DiscHashInfo();

        var files = await fileSystem.Directory.GetFiles(path, pattern);
        var filesInfo = files
            .Select(file => new FileInfo(file))
            .OrderBy(e => e.Name);

        int index = 0;
        foreach (var file in filesInfo)
        {
            info.Files.Add(new FileHashInfo
            {
                Index = index++,
                Name = file.Name,
                CreationTime = file.CreationTime,
                Size = file.Length
            });
        }

        info.Hash = info.Files.CalculateHash();

        return info;
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
}