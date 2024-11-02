namespace TheDiscDb.InputModels
{
    public class Track
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public int Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        // Video Tracks
        public string Resolution { get; set; }
        public string AspectRatio { get; set; }

        // Audio and Subtitle Tracks
        public string AudioType { get; set; }
        public string LanguageCode { get; set; }
        public string Language { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public Title Title { get; set; }
    }
}
