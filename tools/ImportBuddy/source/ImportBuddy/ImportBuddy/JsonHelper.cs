using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using Fantastic.TheMovieDb;
using TheDiscDb.Imdb;

namespace ImportBuddy;

public static class JsonHelper
{
    public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull,
        TypeInfoResolver = JsonTypeInfoResolver.Combine(TheDiscDb.CoreSourceGenerationContext.Default, ImportBuddySourceGenerationContext.Default, TheMovieDbSourceGenerationContext.Default, ImdbSourceGenerationContext.Default)
    };
}

[JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(Fantastic.TheMovieDb.Models.Movie))]
[JsonSerializable(typeof(Fantastic.TheMovieDb.Models.Series))]
internal partial class ImportBuddySourceGenerationContext : JsonSerializerContext
{
}