using System.CommandLine;
using System.Runtime.InteropServices;
using ImportBuddy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using TheDiscDb.Tools.MakeMkv;

// Examples
// hash <pathToDiscFile> <driveLetter>
// finalize <pathToReleaseFolder>

var builder = CreateHostBuilder(args);
var host = CreateHostBuilder(args).Build();
var source = new CancellationTokenSource();

// kick off the cache loading
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
host.Services.GetRequiredService<DiscContentHashCache>().InitializeAsync(source.Token).ContinueWith(t =>
{
    if (t.IsFaulted)
    {
        AnsiConsole.WriteException(t.Exception!);
    }
}, TaskContinuationOptions.OnlyOnFaulted);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

var rootCommand = new RootCommand();

var finalizeCommand = new Command("finalize", "Run finalize on a release");
var releaseFileArgument = new Argument<string>("file", "The release folder to finalize");
var recursiveOption = new Option<bool>(name: "--recursive", description: "Recursively finalize all subfolders", getDefaultValue: () => false)
{
    IsRequired = false
};
finalizeCommand.AddArgument(releaseFileArgument);
finalizeCommand.AddOption(recursiveOption);
finalizeCommand.SetHandler(async (releasePath, recurse) =>
{
    var finalizeTask = host.Services.GetRequiredService<FinalizeTask>();
    await finalizeTask.ExecuteAsync(releasePath, recurse, source.Token);
}, releaseFileArgument, recursiveOption);

var hashCommand = new Command("hash", "Run hash on a disc file");
var discFileArgument = new Argument<string>("file", "The disc json file to update");
var driveLetterArgument = new Argument<string>("drive", "The letter of the drive to scan");
hashCommand.AddArgument(discFileArgument);
hashCommand.AddArgument(driveLetterArgument);
hashCommand.SetHandler(async (discPath, driveLetter) =>
{
    var hashTask = host.Services.GetRequiredService<DiscContentHashTask>();
    await hashTask.SingleDiskMode(new TheDiscDb.Tools.MakeMkv.Drive { Letter = driveLetter }, discPath, source.Token);
}, discFileArgument, driveLetterArgument);

var cleanLogsCommand = new Command("clean-logs", "Clean up the logs");
var logFileArgument = new Argument<string>("file", "The full path to the log file");
var driveIndexArgument = new Argument<int>("driveIndex", () => 0, "The letter of the drive to scan");

cleanLogsCommand.AddArgument(logFileArgument);
cleanLogsCommand.AddArgument(driveIndexArgument);
cleanLogsCommand.SetHandler(async (logFilePath, driveIndex) =>
{
    var helper = host.Services.GetRequiredService<MakeMkvHelper>();
    await helper.CleanLogs(driveIndex, logFilePath, source.Token);
}, logFileArgument, driveIndexArgument);

rootCommand.AddCommand(finalizeCommand);
rootCommand.AddCommand(hashCommand);
rootCommand.AddCommand(cleanLogsCommand);
rootCommand.SetHandler(async () =>
{
    var shell = host.Services.GetRequiredService<Shell>();
    await shell.RunAsync(source);
});

return await rootCommand.InvokeAsync(args);

static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    config.AddJsonFile("appsettings.linux.json", optional: false);
                }
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
            })
            .ConfigureServices((hostContext, services) =>
            {
                var startup = new Startup(hostContext.Configuration, hostContext.HostingEnvironment.ContentRootPath);
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
                startup.ConfigureServices(services);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            });