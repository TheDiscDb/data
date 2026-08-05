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

        // Sub-categories of Extra. Each is bucketed separately so the
        // finalize step can preserve the granular Type when writing back to
        // the disc file, while still being treated as Extras by anything
        // that calls ItemTypeNames.IsExtra(...).
        public ICollection<DiscFileItem> Others { get; set; } = new HashSet<DiscFileItem>();
        public ICollection<DiscFileItem> Interviews { get; set; } = new HashSet<DiscFileItem>();
        public ICollection<DiscFileItem> Featurettes { get; set; } = new HashSet<DiscFileItem>();
        public ICollection<DiscFileItem> Scenes { get; set; } = new HashSet<DiscFileItem>();
        public ICollection<DiscFileItem> Musics { get; set; } = new HashSet<DiscFileItem>();
        public ICollection<DiscFileItem> Shorts { get; set; } = new HashSet<DiscFileItem>();

        public ICollection<DiscFileItem> Unknown { get; set; } = new HashSet<DiscFileItem>();
    }
}
