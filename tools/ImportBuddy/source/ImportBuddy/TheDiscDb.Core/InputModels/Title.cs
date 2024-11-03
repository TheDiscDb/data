namespace TheDiscDb.InputModels
{
    public class Title : DiscItem
    {
        public int Index { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Disc? Disc { get; set; }
    }
}
