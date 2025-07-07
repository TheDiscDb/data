namespace ImportBuddy;

public class ImportBuddyOptions
{
    public string? DataRepositoryPath { get; set; }
    public CachingOptions Caching { get; set; } = new CachingOptions();
}

public class CachingOptions
{
    public bool Enabled { get; set; } = true;
    public bool EnableLocalCache { get; set; } = true;
    public bool EnableRemoteCache { get; set; } = true;
}
