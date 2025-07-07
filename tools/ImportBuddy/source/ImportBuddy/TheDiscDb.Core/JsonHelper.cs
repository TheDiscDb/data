namespace TheDiscDb
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public static class JsonHelper
    {
        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions 
        { 
            WriteIndented = true, 
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull
        };
    }
}
