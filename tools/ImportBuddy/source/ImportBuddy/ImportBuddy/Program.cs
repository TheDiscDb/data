using System.CommandLine;
using System.Runtime.InteropServices;
using ImportBuddy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Examples
// hash <pathToDiscFile> <driveLetter>
// finalize <pathToReleaseFolder>

var builder = CreateHostBuilder(args);
var host = CreateHostBuilder(args).Build();
var source = new CancellationTokenSource();

var rootCommand = new RootCommand();

var finalizeCommand = new Command("finalize", "Run finalize on a release");
var releaseFileArgument = new Argument<string>("file", "The release folder to finalize");
var recursiveOption = new Option<bool>(name: "--recursive", description: "Recursively finalize all subfolders", getDefaultValue: () => false)
{
    IsRequired = false
};
finalizeCommand.AddArgument(releaseFileArgument);
finalizeCommand.AddOption(recursiveOption);

rootCommand.AddCommand(finalizeCommand);

finalizeCommand.SetHandler(async (releasePath, recurse) =>
{
    var finalizeTask = host.Services.GetRequiredService<FinalizeTask>();
    await finalizeTask.ExecuteAsync(releasePath, recurse, source.Token);
}, releaseFileArgument, recursiveOption);

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