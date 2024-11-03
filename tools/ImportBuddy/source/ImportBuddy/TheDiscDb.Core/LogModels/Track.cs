namespace TheDiscDb.LogModels
{
    public class Track
    {
        public int Index { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }

        // Audio Specific
        public string? AudioType { get; set; }
        public string? LanguageCode { get; set; }
        public string? Language { get; set; }

        // Video Specific
        public string? Resolution { get; set; }
        public string? AspectRatio { get; set; }
    }
}
