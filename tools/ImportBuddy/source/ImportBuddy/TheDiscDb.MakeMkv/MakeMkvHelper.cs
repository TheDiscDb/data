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
                    await CleanLogs(path);
                }
                catch (IOException e)
                {
                    throw new CleanLogFileException(path, e);
                }
            }
        }

        public async Task CleanLogs(string path, CancellationToken cancellationToken = default)
        {
            var output = new List<string>();
            var lines = await this.fileSystem.File.ReadAllLines(path, cancellationToken);
            bool fileChanged = false;
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("MSG") || line.StartsWith("DRV"))
                {
                    fileChanged = true;
                    continue;
                }

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
