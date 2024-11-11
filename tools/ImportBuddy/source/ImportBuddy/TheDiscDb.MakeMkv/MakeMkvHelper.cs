using MakeMkv;

namespace TheDiscDb.Tools.MakeMkv
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Fantastic.FileSystem;
    using Microsoft.Extensions.Options;

    public class MakeMkvHelper
    {
        private readonly IOptions<MakeMkvOptions> options;
        private readonly IFileSystem fileSystem;

        public MakeMkvHelper(IOptions<MakeMkvOptions> options, IFileSystem fileSystem)
        {
            this.options = options ?? throw new System.ArgumentNullException(nameof(options));
            this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

            if (options.Value.Path == null)
            {
                throw new Exception("The MakeMKV Path is not configured");
            }
        }

        public IList<Drive> Drives => this.options.Value.Drives;

        public async Task WriteLogs(int driveIndex, string path, bool cleanLogs = true)
        {
            var info = new ProcessStartInfo
            {
                FileName = this.options.Value.Path,
                Arguments = $"--robot --messages=\"{path}\" info disc:{driveIndex}"
            };

            await RunProcessAsync(info);

            if (cleanLogs)
            {
                await Task.Delay(200); // wait for file handle to be released
                try
                {
                    await CleanLogs(driveIndex, path);
                }
                catch (IOException e)
                {
                    throw new CleanLogFileException(path, e);
                }
            }
        }

        public async Task CleanLogs(int driveIndex, string path, CancellationToken cancellationToken = default)
        {
            var output = new List<string>();
            var lines = await this.fileSystem.File.ReadAllLines(path, cancellationToken);
            bool fileChanged = false;
            foreach (var originalLine in lines)
            {
                // Remove empty lines from the output
                if (originalLine == string.Empty)
                {
                    fileChanged = true;
                    continue;
                }

                string? line = null;
                if (originalLine.StartsWith("MSG"))
                {
                    var msgLine = MessageLogLine.Parse(originalLine);
                    var (toRedact, replacement) = msgLine.Code switch
                    {
                        // The debug file path is usually in the user's home directory
                        "1004" => (msgLine.Params[0], "file::///redacted/by/ImportBuddy"),
                        // The drive name can sometimes contain a serial number
                        "2003" => (msgLine.Params[1], "redacted by ImportBuddy"),
                        // The .MakeMKV folder is usually in the user's home directory
                        "3338" => (msgLine.Params[1], "/redacted/by/ImportBuddy"),
                        // No other messages are known to contain sensitive information
                        _ => (null, ""),
                    };

                    if (toRedact is not null)
                        line = originalLine.Replace(toRedact, replacement);
                }
                else if (originalLine.StartsWith("DRV"))
                {
                    // DRV:1,2,999,12,"BD-ROM HL-DT-ST BDDVDRW UH12NS30 1.03","42","E:"
                    // becomes
                    // DRV:1,2,999,12,"redacted by ImportBuddy","42","/redacted/by/ImportBuddy"
                    var driveLine = DriveScanLogLine.Parse(originalLine);
                    if (!string.IsNullOrEmpty(driveLine.DriveName))
                    {
                        // Always redact the drive name, since it could contain a serial number
                        line = originalLine.Replace(driveLine.DriveName, "redacted by ImportBuddy");
                        // Only keep the disc name for the active drive
                        if (driveLine.Index != driveIndex && !string.IsNullOrEmpty(driveLine.DiscName))
                            line = line.Replace(driveLine.DiscName, "redacted by ImportBuddy");
                        // Always redact the drive letter or path
                        if (!string.IsNullOrEmpty(driveLine.DriveLetter))
                            line = line.Replace(driveLine.DriveLetter, "/redacted/by/ImportBuddy");
                    }
                }

                if (line is null)
                    // This line wasn't modified, use the original line
                    line = originalLine;
                else
                    // Something was changed, ensure the file is overwritten
                    fileChanged = true;

                output.Add(line);
            }

            if (fileChanged)
            {
                await this.fileSystem.File.WriteAllLines(path, output, cancellationToken);
            }
        }

        private static Task<int> RunProcessAsync(ProcessStartInfo info)
        {
            var tcs = new TaskCompletionSource<int>();

            var process = new Process
            {
                StartInfo = info,
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }
    }

    public class CleanLogFileException : ApplicationException
    {
        public CleanLogFileException(string path, Exception? innerException)
            : base($"Unable to clean log for '{path}'", innerException)
        {
        }
    }
}
