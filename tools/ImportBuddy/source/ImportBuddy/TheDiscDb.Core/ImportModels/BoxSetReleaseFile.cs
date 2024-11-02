namespace TheDiscDb.ImportModels
{
    using System;
    using System.Collections.Generic;

    public class BoxSetReleaseFile
    {
        public const string Filename = "boxset.json";
        
        public string Slug { get; set; }
        public string Asin { get; set; }
        public string Upc { get; set; }
        public int Year { get; set; }
        public string Locale { get; set; }
        public string RegionCode { get; set; }
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public string Isbn { get; set; }
        public string Type { get; set; }
        public string ImageUrl { get; set; }
        public DateTimeOffset ReleaseDate { get; set; }
        public DateTimeOffset DateAdded { get; set; }
        public ICollection<BoxSetDisc> Discs { get; set; } = new HashSet<BoxSetDisc>();
    }
}
