namespace TheDiscDb
{
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using TheDiscDb.ImportModels;
    using TheDiscDb.InputModels;

    public static class JsonHelper
    {
        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions 
        { 
            WriteIndented = true, 
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull,
            TypeInfoResolver = CoreSourceGenerationContext.Default
        };
    }

    [JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(MetadataFile))]
    [JsonSerializable(typeof(ReleaseFile))]
    [JsonSerializable(typeof(BoxSetReleaseFile))]
    [JsonSerializable(typeof(Disc))]
    [JsonSerializable(typeof(DiscFile))]
    public partial class CoreSourceGenerationContext : JsonSerializerContext
    {
    }
}
