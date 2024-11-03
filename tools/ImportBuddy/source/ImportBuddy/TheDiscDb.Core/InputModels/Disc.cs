using TheDiscDb.InputModels;

namespace TheDiscDb.InputModels
{
    using System.Collections.Generic;

    public interface IDisc
    {
        int Index { get; }
        string? Slug { get; }
        string? Name { get; }
        string? Format { get; }
    }

    public class Disc : IDisc
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public int Id { get; set; }
        public int Index { get; set; }
        public string? Slug { get; set; }
        public string? Name { get; set; }
        public string? Format { get; set; }
        public string? ContentHash { get; set; }

        [HotChocolate.Data.UseFiltering]
        [HotChocolate.Data.UseSorting]
        public ICollection<Title> Titles { get; set; } = new HashSet<Title>();
        [System.Text.Json.Serialization.JsonIgnore]
        public Release? Release { get; set; }
    }
}

public static class DiscExtensions
{
    public static string SlugOrIndex(this IDisc disc)
    {
        if (!string.IsNullOrEmpty(disc.Slug))
        {
            return disc.Slug;
        }

        return disc.Index.ToString();
    }
}
