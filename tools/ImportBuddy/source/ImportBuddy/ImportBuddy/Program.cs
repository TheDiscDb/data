using ImportBuddy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Examples
// -hash <pathToDiscFile> <driveLetter>

var host = CreateHostBuilder(args).Build();
var shell = Bootstrap();
var source = new CancellationTokenSource();
await shell.RunAsync(source);

static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                
            });

static Shell Bootstrap()
{
    ServiceCollection serviceCollection = new ServiceCollection();

    string baseDir = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
    IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
        .SetBasePath(baseDir)
        .AddJsonFile("appsettings.json", optional: false)
        .AddUserSecrets(typeof(Startup).Assembly);

    IConfiguration configuration = configurationBuilder.Build();

    var startup = new Startup(configuration, baseDir);
    startup.ConfigureServices(serviceCollection);
    serviceCollection.AddSingleton<Shell>();
    serviceCollection.AddSingleton<IServiceCollection>(serviceCollection);
    ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

    return serviceProvider.GetRequiredService<Shell>();
}