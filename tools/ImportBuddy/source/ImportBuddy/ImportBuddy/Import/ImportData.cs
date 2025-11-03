using TheDiscDb.ImportModels;
using TheDiscDb.Tools.MakeMkv;
using TheDiscDb.Core.DiscHash;
using TheDiscDb.Import;

namespace ImportBuddy;

public class ImportData
{
    public Drive? Drive { get; set; }
    public DiscHashInfo? HashInfo { get; set; }
    public DiscContentHashCacheItem? ExistingDisc { get; set; }
    public ImportItemType? ItemType { get; set; }
    public ImportItem? ImportItem { get; set; }
    public string? DiscFormat { get; set; }
    public MetadataFile? Metadata { get; set; }
    public string? OutputDirectory { get; set; }

    public bool ExistingDiscFound => this.ExistingDisc != null;

    public static ImportItemType GetItemType(string type)
    {
        return type switch
        {
            "Movie" => ImportItemType.Movie,
            "Series" => ImportItemType.Series,
            "Boxset" => ImportItemType.Boxset,
            _ => throw new ArgumentException("Invalid type", nameof(type)),
        };
    }
}
