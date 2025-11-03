using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantastic.FileSystem;
using MakeMkv;
using TheDiscDb.Imdb;
using TheDiscDb.ImportModels;

namespace TheDiscDb.Import;

public static class ImportHelper
{
    public static string CreateSlug(string name, int year)
    {
        if (year != default(int))
        {
            return string.Format("{0}-{1}", name.Slugify(), year);
        }

        return name.Slugify();
    }

    public static string? GetSortTitle(string? title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return title;
        }

        if (title.StartsWith("the", StringComparison.OrdinalIgnoreCase))
        {
            return title.Substring(4, title.Length - 4).Trim() + ", The";
        }
        else
        {
            return title;
        }
    }

    public struct DiscName
    {
        public string Name;
        public int Index;
    }

    public static async Task<DiscName> GetDiscName(this IFileSystem fileSystem, string path)
    {
        var files = await fileSystem.Directory.GetFiles(path, "*disc*");
        var name = new DiscName
        {
            Name = "disc01",
            Index = 1
        };

        for (int i = 1; i < 100; i++)
        {
            name.Name = string.Format("disc{0:00}", i);
            name.Index = i;

            if (files.Any(f => f.Contains(name.Name)))
            {
                continue;
            }

            break;
        }

        return name;
    }

    public static MetadataFile BuildMetadata(TitleData? imdbTitle, Fantastic.TheMovieDb.Models.Movie? movie, Fantastic.TheMovieDb.Models.Series? series, int year, ImportItemType type)
    {
        var metadata = new MetadataFile
        {
            Year = year,
            Type = type.ToString(),
            DateAdded = DateTimeOffset.UtcNow.Date
        };

        if (imdbTitle != null && string.IsNullOrEmpty(imdbTitle.ErrorMessage))
        {
            metadata.Title = imdbTitle.Title;
            metadata.FullTitle = imdbTitle.FullTitle;
            metadata.ExternalIds.Imdb = imdbTitle.Id;
            if (imdbTitle?.Title != null)
            {
                metadata.Slug = CreateSlug(imdbTitle.Title, year);
            }
        }
        else if (movie != null)
        {
            if (movie.ReleaseDate.HasValue)
            {
                metadata.ReleaseDate = movie.ReleaseDate.Value;
            }

            metadata.Title = movie.Title;
            metadata.FullTitle = movie.OriginalTitle;

            if (movie?.Title != null)
            {
                metadata.Slug = CreateSlug(movie.Title, year);
            }

            if (string.IsNullOrEmpty(metadata.ExternalIds.Imdb))
            {
                metadata.ExternalIds.Imdb = movie!.ImdbId;
                if (string.IsNullOrEmpty(metadata.ExternalIds.Imdb))
                {
                    metadata.ExternalIds.Imdb = movie!.ExternalIds?.ImdbId;
                }
            }
        }
        else if (series != null)
        {
            if (series.FirstAirDate.HasValue)
            {
                metadata.ReleaseDate = series.FirstAirDate.Value;
            }

            metadata.Title = series.Name;
            metadata.SortTitle = GetSortTitle(series.Name);
            metadata.SortTitle = GetSortTitle(metadata.Title);
            metadata.FullTitle = series.OriginalName;

            if (series?.Name != null)
            {
                metadata.Slug = CreateSlug(series.Name, year);
            }

            if (string.IsNullOrEmpty(metadata.ExternalIds.Imdb))
            {
                metadata.ExternalIds.Imdb = series!.ExternalIds?.ImdbId;
            }
        }

        if (imdbTitle != null && string.IsNullOrEmpty(imdbTitle.ErrorMessage))
        {
            metadata.Plot = imdbTitle.Plot;
            metadata.Directors = imdbTitle.Directors;
            metadata.Stars = imdbTitle.Stars;
            metadata.Writers = imdbTitle.Writers;
            metadata.Genres = imdbTitle.Genres;
            metadata.Runtime = imdbTitle.RuntimeStr;
            metadata.ContentRating = imdbTitle.ContentRating;
            metadata.Tagline = imdbTitle.Tagline;
            if (metadata.ReleaseDate == default(DateTimeOffset) && !string.IsNullOrEmpty(imdbTitle.ReleaseDate))
            {
                metadata.ReleaseDate = DateTimeOffset.Parse(imdbTitle.ReleaseDate + "T00:00:00+00:00");
            }

            if (Int32.TryParse(imdbTitle.RuntimeMins, out int minutes))
            {
                metadata.RuntimeMinutes = minutes;
            }
        }

        if (movie != null)
        {
            metadata.ExternalIds.Tmdb = movie.Id.ToString();

            if (string.IsNullOrEmpty(metadata.Plot))
            {
                metadata.Plot = movie.Overview;
            }

            if (string.IsNullOrEmpty(metadata.Tagline))
            {
                metadata.Tagline = movie.Tagline;
            }
        }
        else if (series != null)
        {
            metadata.ExternalIds.Tmdb = series.Id.ToString();

            if (string.IsNullOrEmpty(metadata.Plot))
            {
                metadata.Plot = series.Overview;
            }
        }

        if (metadata.Title != null && metadata.Title.StartsWith("the", StringComparison.OrdinalIgnoreCase))
        {
            metadata.SortTitle = metadata.Title.Substring(4, metadata.Title.Length - 4).Trim() + ", The";
        }
        else
        {
            metadata.SortTitle = metadata.Title;
        }

        return metadata;
    }
}

public static class DiscFileFinalizer
{
    /// <summary>Finalize a disc json</summary>
    /// <param name="disc">Represents the discXX.json file</param>
    /// <param name="discFile">Represents and organized and parsed discXX-summary.txt</param>
    /// <param name="discInfo">Represesnts the parsed contents of the MakeMkv log file discXX.txt</param>
    public static void Map(TheDiscDb.InputModels.Disc disc, DiscFile discFile, DiscInfo discInfo)
    {
        var mapper = new Mapper();

        var logDisc = mapper.Map(discInfo);

        if (disc.Index == default(int))
        {
            disc.Index = discFile.Index;
        }

        if (string.IsNullOrEmpty(disc.Name))
        {
            disc.Name = logDisc.Name;
        }

        if (string.IsNullOrEmpty(disc.Format))
        {
            disc.Format = discFile.Format;
        }

        if (string.IsNullOrEmpty(disc.ContentHash))
        {
            disc.ContentHash = discFile.ContentHash;
        }

        disc.Titles = logDisc.Titles.Select(mapper.Map).ToList();

        if (discFile.Unknown.Any())
        {
            throw new Exception($"There are unknown items in this summary (Disc {discFile.Index})");
        }

        TryMapItems(discFile.Episodes, disc, "Episode");
        TryMapItems(discFile.Extras, disc, "Extra");
        TryMapItems(discFile.DeletedScenes, disc, "DeletedScene");
        TryMapItems(discFile.Trailers, disc, "Trailer");
        TryMapItems(discFile.MainMovies, disc, "MainMovie");
    }

    private static void TryMapItems(ICollection<TheDiscDb.ImportModels.DiscFileItem> items, TheDiscDb.InputModels.Disc disc, string type)
    {
        foreach (var item in items)
        {
            if (!string.IsNullOrEmpty(item.Title) && item.Title.Contains(" (English)", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Language detected in '{item.Title}'");
            }

            var matchingTitles = disc.Titles.Where(title => title.SegmentMap == item.SegmentMap && title.SourceFile == item.SourceFile && item.Duration == title.Duration);
            if (!string.IsNullOrEmpty(disc.Format) && disc.Format.Equals("dvd", StringComparison.OrdinalIgnoreCase))
            {
                matchingTitles = matchingTitles.Where(title => title.DisplaySize == item.Size);
            }

            if (matchingTitles.Count() > 0)
            {
                var match = matchingTitles.First();
                if (matchingTitles.Count() > 1)
                {
                    // try to select based on comment
                    var primaryMatch = matchingTitles.SingleOrDefault(title => !string.IsNullOrEmpty(title.Comment) && title.Comment == item.Comment);
                    if (primaryMatch != null)
                    {
                        match = primaryMatch;
                    }
                    else
                    {
                        throw new Exception($"Unable to find unique match for '{item.Comment}'");
                    }
                }

                if (item.AudioTrackNames != null && item.AudioTrackNames.Any())
                {
                    foreach (var track in item.AudioTrackNames)
                    {
                        var audioTracks = match.Tracks.Where(t => t.Type == "Audio");
                        var foundTrack = audioTracks.ElementAtOrDefault(track.Index);
                        if (foundTrack != null)
                        {
                            foundTrack.Description = track.Name;
                        }
                        else
                        {
                            throw new Exception($"Unable to find audio track '{track.Name}' at index '{track.Index}'. {audioTracks.Count()} total audio tracks found.");
                        }
                    }
                }

                match.Item = new TheDiscDb.InputModels.DiscItemReference
                {
                    Title = item.Title,
                    Type = type,
                    Chapters = item.Chapters,
                    Season = item.Season,
                    Episode = item.Episode,
                    Description = item.Description
                };
            }
        }
    }
}

public class Mapper
{
    public TheDiscDb.InputModels.Title Map(TheDiscDb.LogModels.Title title)
    {
        return new TheDiscDb.InputModels.Title
        {
            DisplaySize = title.DisplaySize,
            Index = title.Index,
            Duration = title.Length,
            SourceFile = title.Playlist,
            SegmentMap = title.SegmentMap,
            Size = title.Size,
            Comment = title.Comment,
            Tracks = title.Tracks.Select(this.Map).ToList()
        };
    }

    public TheDiscDb.InputModels.Track Map(TheDiscDb.LogModels.Track track)
    {
        return new TheDiscDb.InputModels.Track
        {
            AspectRatio = track.AspectRatio,
            AudioType = track.AudioType,
            Index = track.Index,
            Language = track.Language,
            LanguageCode = track.LanguageCode,
            Name = track.Name,
            Resolution = track.Resolution,
            Type = track.Type
        };
    }

    public TheDiscDb.LogModels.Disc Map(DiscInfo item)
    {
        var disc = new TheDiscDb.LogModels.Disc
        {
            Name = item.Name,
            Media = item.Type,
            Language = item.Language
        };

        foreach (var title in item.Titles)
        {
            var t = new TheDiscDb.LogModels.Title
            {
                Index = title.Index,
                DisplaySize = title.DisplaySize,
                SegmentMap = title.SegmentMap,
                Comment = title.Comment,
                Size = title.Size,
                Playlist = title.Playlist,
                Length = title.Length
            };

            foreach (var segment in title.Segments)
            {
                t.Tracks.Add(new TheDiscDb.LogModels.Track
                {
                    AspectRatio = segment.AspectRatio,
                    AudioType = segment.AudioType,
                    Index = segment.Index,
                    Language = segment.Language,
                    LanguageCode = segment.LanguageCode,
                    Name = segment.Name,
                    Resolution = segment.Resolution,
                    Type = segment.Type
                });
            }

            disc.Titles.Add(t);
        }

        return disc;
    }
}