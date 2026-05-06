namespace TheDiscDb.InputModels;

public class Contributor
{
    [System.Text.Json.Serialization.JsonIgnore]
    public int Id { get; set; }
    public string? Name { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Release> Releases { get; set; } = new List<Release>();
    [System.Text.Json.Serialization.JsonIgnore]
    public string? UserId { get; set; }

    // Github, Google, etc
    public string? Source { get; set; }
}
