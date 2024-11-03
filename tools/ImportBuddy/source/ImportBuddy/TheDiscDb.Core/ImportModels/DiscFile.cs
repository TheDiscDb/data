namespace TheDiscDb.ImportModels
{
    using System.Collections.Generic;

    public class DiscFile
    {
        public int Index { get; set; }
        public string? Format { get; set; }
        public string? Slug { get; set; }
        public string? Name { get; set; }
        public string? ContentHash { get; set; }

        public ICollection<DiscFileItem> Episodes { get; set; } = new HashSet<DiscFileItem>();
        public ICollection<DiscFileItem> Extras { get; set; } = new HashSet<DiscFileItem>();
        public ICollection<DiscFileItem> DeletedScenes { get; set; } = new HashSet<DiscFileItem>();
        public ICollection<DiscFileItem> Trailers { get; set; } = new HashSet<DiscFileItem>();
        public ICollection<DiscFileItem> MainMovies { get; set; } = new HashSet<DiscFileItem>();
        public ICollection<DiscFileItem> Unknown { get; set; } = new HashSet<DiscFileItem>();
    }
}
