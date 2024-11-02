using System;
namespace MakeMkv;

public class HashInfoLogLine : LogLine
{
    public const string LinePrefix = "HSH";

    public HashInfoLogLine() : base(LinePrefix)
    {
    }

    public int Index { get; set; }
    public string Name { get; set; }
    public DateTime CreationTime { get; set; }
    public long Size { get; set; }

    public static HashInfoLogLine Parse(string line)
    {
        string[] parts = line.Substring(4).Split(',');

        var result = new HashInfoLogLine
        {
            OriginalLine = line,
            Index = TryParseInt(0, parts),
            Name = GetString(1, parts),
            CreationTime = TryParseDateTime(2, parts),
            Size = TryParseLong(3, parts)
        };

        return result;
    }

    public override string ToString()
    {
        return $"{LinePrefix}:{Index},{Name},{CreationTime},{Size}";
    }
}
