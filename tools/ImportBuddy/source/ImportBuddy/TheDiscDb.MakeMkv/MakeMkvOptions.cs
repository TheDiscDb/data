namespace TheDiscDb.Tools.MakeMkv
{
    using System.Collections.Generic;

    public class MakeMkvOptions
    {
        public string? Path { get; set; }
        public IList<Drive> Drives { get; set; } = new List<Drive>();
    }
}
