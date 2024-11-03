namespace TheDiscDb.LogModels
{
    using System.Collections.Generic;

    public class Release
    {
        public string? Name { get; set; }
        public string? Locale { get; set; }
        public IList<Disc> Discs { get; set; } = new List<Disc>();
    }
}
