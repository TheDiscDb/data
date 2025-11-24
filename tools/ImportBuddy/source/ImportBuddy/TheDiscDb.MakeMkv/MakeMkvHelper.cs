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
            var lines = await this.fileSystem.File.ReadAllLines(path, cancellationToken);

            var output = LogParser.CleanLogs(lines, out bool fileChanged);

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
