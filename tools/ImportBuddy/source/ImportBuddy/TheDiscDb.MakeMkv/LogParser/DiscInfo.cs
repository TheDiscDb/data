namespace MakeMkv
{
    using System.Collections.Generic;

    public class DiscInfo
    {
        public const int TypeId = 1;
        public const int NameId = 2;
        public const int LanguageCodeId = 28;
        public const int LanguageId = 29;

        public string? Name { get; set; } //2
        public string? Type { get; set; } //1
        public string? LanguageCode { get; set; } //28
        public string? Language { get; set; } //29
        public IList<Title> Titles { get; set; } = new List<Title>();
        public IList<HashInfoLogLine> HashInfo { get; set; } = new List<HashInfoLogLine>();
    }
}
