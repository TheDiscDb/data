namespace TheDiscDb.Core.DiscHash;

public record FileHashInfo
{
    public int Index { get; set; }
    public string? Name { get; set; }
    public DateTime CreationTime { get; set; }
    public long Size { get; set; }
}
