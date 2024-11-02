namespace TheDiscDb.LogModels
{
    using System.Collections.Generic;

    public class Disc
    {
        public string Name { get; set; }
        public string Language { get; set; }
        public string Media { get; set; }

        public IList<Title> Titles { get; set; } = new List<Title>();
    }
}
