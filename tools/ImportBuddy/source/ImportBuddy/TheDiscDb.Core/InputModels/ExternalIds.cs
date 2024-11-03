namespace TheDiscDb.InputModels
{
    public class ExternalIds
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public int Id { get; set; }
        public string? Tmdb { get; set; }
        public string? Imdb { get; set; }
        public string? Tvdb { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public MediaItem? MediaItem { get; set; }
    }
}
