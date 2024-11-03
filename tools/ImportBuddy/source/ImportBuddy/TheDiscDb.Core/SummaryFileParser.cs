namespace TheDiscDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using TheDiscDb.ImportModels;
    using TheDiscDb.InputModels;

    public class SummaryFileParser
    {
        private static Regex AudioTrackPattern = new Regex(@"AudioTrack\[(\d+)\]:\s+(.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static IEnumerable<Chapter> ParseChapters(string input)
        {
            int i = 1;
            foreach (string line in input.Split(Environment.NewLine).Skip(1)) // skip the first line
            {
                yield return new Chapter
                {
                    Index = i++,
                    Title = line
                };
            }
        }

        public static DiscFile ParseSingleDisc(string input)
        {
            var disc = new DiscFile
            {
                Index = 1
            };

            var items = Parse(input);
            Categorize(disc, items);

            return disc;
        }

        public static IEnumerable<DiscFile> ParseDiscs(string input)
        {
            DiscFile? current = null;
            var currentLines = new List<string>();
            int index = 1;

            foreach (string line in input.Split(Environment.NewLine))
            {
                if (line.StartsWith("---"))
                {
                    if (current != null)
                    {
                        var items = Parse(currentLines);
                        Categorize(current, items);
                        yield return current;
                    }

                    // Start new disc
                    current = new DiscFile
                    {
                        Index = index++
                    };
                    currentLines.Clear();
                    continue;
                }

                currentLines.Add(line);
            }
        }

        private static void Categorize(DiscFile disc, IEnumerable<DiscFileItem> items)
        {
            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.Type))
                {
                    disc.Unknown.Add(item);
                    continue;
                }

                if (item.Type.Equals("Extra", StringComparison.OrdinalIgnoreCase))
                {
                    disc.Extras.Add(item);
                }
                else if (item.Type.Equals("Episode", StringComparison.OrdinalIgnoreCase))
                {
                    disc.Episodes.Add(item);
                }
                else if (item.Type.Equals("DeletedScene", StringComparison.OrdinalIgnoreCase))
                {
                    disc.DeletedScenes.Add(item);
                }
                else if (item.Type.Equals("Trailer", StringComparison.OrdinalIgnoreCase))
                {
                    disc.Trailers.Add(item);
                }
                else if (item.Type.Equals("MainMovie", StringComparison.OrdinalIgnoreCase))
                {
                    disc.MainMovies.Add(item);
                }
                else
                {
                    disc.Unknown.Add(item);
                }
            }
        }

        public static IEnumerable<DiscFileItem> Parse(IEnumerable<string> lines)
        {
            return Parse(string.Join(Environment.NewLine, lines));
        }

        private static bool TryParse(string line, out KeyValuePair<string, string> result)
        {
            if (line.StartsWith("Chapters:", StringComparison.OrdinalIgnoreCase))
            {
                result = new KeyValuePair<string, string>("Chapters", string.Empty);
                return true;
            }
            else if (line.StartsWith("-", StringComparison.OrdinalIgnoreCase))
            {
                result = new KeyValuePair<string, string>("-", line.Replace("-", "").Trim());
                return true;
            }

            if (line.StartsWith("AudioTrack"))
            {
                Match matchResult = AudioTrackPattern.Match(line);
                if (matchResult.Success)
                {
                    string index = matchResult.Groups[1].Value;
                    string name = matchResult.Groups[2].Value;
                    result = new KeyValuePair<string, string>("AudioTrack", $"{index}|${name}");
                    return true;
                }
            }

            int splitAt = line.IndexOf(':');
            if (splitAt < 0)
            {
                result = default(KeyValuePair<string, string>);
                return false;
            }
            string key = line.Substring(0, splitAt);

            if (line.Length < splitAt + 2)
            {
                // A key without a value
                result = default(KeyValuePair<string, string>);
                return false;
            }

            string value = line.Substring(splitAt + 2);
            result = new KeyValuePair<string, string>(key, value);
            return true;
        }

        public static IEnumerable<DiscFileItem> Parse(string input)
        {
            var title = new DiscFileItem();

            int index = 0;
            bool collectingChapters = false;
            int currentChapter = 1;

            foreach (string line in input.Split(Environment.NewLine))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (TryParse(line, out KeyValuePair<string, string> result))
                {
                    if (result.Key.Equals("Comment", StringComparison.OrdinalIgnoreCase))
                    {
                        CheckChaptersEnded();
                        title.Comment = result.Value;
                    }
                    else if (result.Key.Equals("Source file name", StringComparison.OrdinalIgnoreCase) || result.Key.Equals("Source title ID", StringComparison.OrdinalIgnoreCase))
                    {
                        CheckChaptersEnded();
                        title.SourceFile = result.Value;
                    }
                    else if (result.Key.Equals("Duration", StringComparison.OrdinalIgnoreCase))
                    {
                        CheckChaptersEnded();
                        title.Duration = result.Value;
                    }
                    else if (result.Key.Equals("Size", StringComparison.OrdinalIgnoreCase))
                    {
                        CheckChaptersEnded();
                        title.Size = result.Value;
                    }
                    else if (result.Key.Equals("Segment map", StringComparison.OrdinalIgnoreCase))
                    {
                        CheckChaptersEnded();
                        title.SegmentMap = result.Value;
                    }
                    else if (result.Key.Equals("Season", StringComparison.OrdinalIgnoreCase))
                    {
                        CheckChaptersEnded();
                        title.Season = result.Value;
                    }
                    else if (result.Key.Equals("Episode", StringComparison.OrdinalIgnoreCase))
                    {
                        CheckChaptersEnded();
                        title.Episode = result.Value;
                    }
                    else if (result.Key.Equals("Name", StringComparison.OrdinalIgnoreCase))
                    {
                        CheckChaptersEnded();
                        if (string.IsNullOrEmpty(title.Title))
                        {
                            title.Title = result.Value;
                        }
                    }
                    else if (result.Key.Equals("Description", StringComparison.OrdinalIgnoreCase))
                    {
                        CheckChaptersEnded();
                        if (string.IsNullOrEmpty(title.Description))
                        {
                            title.Description = result.Value;
                        }
                    }
                    else if (result.Key.Equals("Type", StringComparison.OrdinalIgnoreCase))
                    {
                        CheckChaptersEnded();
                        title.Type = result.Value;
                    }
                    else if (result.Key.Equals("Upc", StringComparison.OrdinalIgnoreCase))
                    {
                        CheckChaptersEnded();
                        title.Upc = result.Value;
                    }
                    else if (result.Key.Equals("Year", StringComparison.OrdinalIgnoreCase))
                    {
                        CheckChaptersEnded();
                        if (Int32.TryParse(result.Value, out int year))
                        {
                            title.Year = year;
                        }
                    }
                    else if (result.Key.Equals("AudioTrack", StringComparison.OrdinalIgnoreCase))
                    {
                        CheckChaptersEnded();
                        string[] parts = result.Value.Split('|');
                        if (parts.Length == 2)
                        {
                            int audioTrackIndex = Int32.Parse(parts[0]);
                            string audioTrackName = parts[1];
                            //title.AudioTrackNames.Add(audioTrackIndex, audioTrackName);
                        }
                    }
                    else if (result.Key.Equals("Chapters", StringComparison.OrdinalIgnoreCase))
                    {
                        CheckChaptersEnded();
                        collectingChapters = true;
                    }
                    else if (result.Key.StartsWith('-'))
                    {
                        if (collectingChapters)
                        {
                            title.Chapters.Add(new Chapter
                            {
                                Title = result.Value,
                                Index = currentChapter++
                            });
                        }
                    }
                    else if (result.Key.Equals("File name", StringComparison.OrdinalIgnoreCase)) // This is the last item in set
                    {
                        CheckChaptersEnded();

                        if (string.IsNullOrEmpty(title.Comment))
                        {
                            title.Comment = result.Value;
                        }

                        if (string.IsNullOrEmpty(title.Type) && !string.IsNullOrEmpty(title.Title))
                        {
                            if (title.Title.Contains("trailer", StringComparison.OrdinalIgnoreCase))
                            {
                                title.Type = "Trailer";
                            }
                            else if (title.Title.Contains("deleted", StringComparison.OrdinalIgnoreCase))
                            {
                                title.Type = "DeletedScene";
                            }
                        }

                        yield return title;
                        title = new DiscFileItem();
                        index = -1;
                    }
                }
                else if (index == 0)
                {
                    title.Title = line;
                }

                ++index;
            }

            void CheckChaptersEnded()
            {
                if (collectingChapters)
                {
                    collectingChapters = false;
                    currentChapter = 1;
                }
            }
        }
    }
}
