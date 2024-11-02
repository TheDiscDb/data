namespace TheDiscDb.Core.DiscHash;

public class DiscHashInfo
{
    public string Hash { get; set; }
    public IList<FileHashInfo> Files { get; set; } = new List<FileHashInfo>();
}
