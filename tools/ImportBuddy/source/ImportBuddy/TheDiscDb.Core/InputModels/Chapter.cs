namespace TheDiscDb.InputModels
{
    public class Chapter
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public int Id { get; set; }
        public int Index { get; set; }
        public string? Title { get; set; }
    }
}
