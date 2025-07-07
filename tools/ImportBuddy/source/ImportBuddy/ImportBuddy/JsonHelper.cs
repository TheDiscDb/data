using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImportBuddy;

public static class JsonHelper
{
    public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull
    };
}