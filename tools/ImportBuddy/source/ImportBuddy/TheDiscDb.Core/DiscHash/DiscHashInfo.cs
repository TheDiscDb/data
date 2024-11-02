namespace TheDiscDb.Core.DiscHash;

public record DiscHashInfo(string Hash)
{
    public IList<FileHashInfo> Files { get; set; } = new List<FileHashInfo>();
}
