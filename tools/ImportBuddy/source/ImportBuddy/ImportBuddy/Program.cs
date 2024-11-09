using System.Runtime.InteropServices;
using ImportBuddy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Examples
// -hash <pathToDiscFile> <driveLetter>

var builder = CreateHostBuilder(args);
var host = CreateHostBuilder(args).Build();
var shell = host.Services.GetRequiredService<Shell>();
var source = new CancellationTokenSource();
await shell.RunAsync(source);

static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    config.AddJsonFile("appSettings.linux.json", optional: false);
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