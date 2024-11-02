using Fantastic.FileSystem;
using Fantastic.TheMovieDb;
using Fantastic.TheMovieDb.Caching.FileSystem;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using TheDiscDb.Tools.MakeMkv;

namespace ImportBuddy;

public class Startup
{
    public Startup(IConfiguration configuration, string baseDirectory)
    {
        Configuration = configuration;
        BaseDirectory = baseDirectory;
    }

    public IConfiguration Configuration { get; }
    public string BaseDirectory { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        IFileSystem fileSystem = new PhysicalFileSystem();
        services.AddSingleton(fileSystem);
        services.AddHttpClient();

        services.Configure<ImportBuddyOptions>(this.Configuration.GetSection("ImportBuddy"));

        services.AddSingleton<MakeMkvHelper>();
        services.Configure<MakeMkvOptions>(this.Configuration.GetSection("MakeMkv"));

        services.Configure<TheMovieDbOptions>(this.Configuration.GetSection("TheMovieDb"));
        services.AddSingleton<TheMovieDbClient>();
        services.AddHttpClient<TheMovieDbClient>()
            .AddTransientHttpErrorPolicy(p =>
            {
                return p.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            });
        services.AddFileSystemCache<TheMovieDbClient>(config =>
        {
            config.BaseDirectory = fileSystem.Path.Combine(BaseDirectory, ".TheMovieDbCache");
        });

        services.AddSingleton<IConsoleTask, ImportTask>();
        services.AddSingleton<IConsoleTask, FinalizeTask>();
        services.AddSingleton<IConsoleTask, ExitTask>();

        services.AddSingleton<IImportTask, RecentItemImportTask>();
        services.AddSingleton<IImportTask, TmdbByIdImportTask>();
    }
}
