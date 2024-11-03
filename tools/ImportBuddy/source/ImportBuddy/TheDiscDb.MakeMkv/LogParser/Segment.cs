namespace MakeMkv
{
    public class Segment
    {
        public const int TypeId = 1;
        public const int NameId = 7;
        public const int AudioTypeId = 2;
        public const int LanguageCodeId = 3;
        public const int LanguageId = 4;
        public const int ResolutionId = 19;
        public const int AspectRatioId = 20;

        public int Index { get; set; }
        public string? Type { get; set; } //1
        public string? Name { get; set; } //7

        // Audio Specific
        public string? AudioType { get; set; } //2
        public string? LanguageCode { get; set; } //3
        public string? Language { get; set; } //4

        // Video Specific
        public string? Resolution { get; set; } //19
        public string? AspectRatio { get; set; } //20
    }
}
