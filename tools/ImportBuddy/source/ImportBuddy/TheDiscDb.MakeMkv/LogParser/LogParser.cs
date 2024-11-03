namespace MakeMkv;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class LogParser
{
    private static Dictionary<string, Func<string, LogLine>> lineParsers = new Dictionary<string, Func<string, LogLine>>
    {
        { "#", CommentLogLine.Parse },
        { "DRV", DriveScanLogLine.Parse },
        { "MSG", MessageLogLine.Parse },
        { "TCOUNT", TrackCountLogLine.Parse },
        { "CINFO", SourceInformationLogLine.Parse },
        { "TINFO", TrackInformationLogLine.Parse },
        { "SINFO", SegmentInformationLogLine.Parse },
        { HashInfoLogLine.LinePrefix, HashInfoLogLine.Parse }
    };

    public static IEnumerable<LogLine> Parse(string inputPath)
    {
        if (File.Exists(inputPath))
        {
            return Parse(File.ReadAllLines(inputPath));
        }

        return Enumerable.Empty<LogLine>();
    }

    public static IEnumerable<LogLine> Parse(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            string prefix = "";
            if (line.StartsWith("#"))
            {
                prefix = "#";
            }
            else
            {
                int length = line.IndexOf(':');
                if (length > 0)
                {
                    prefix = line.Substring(0, length);
                }
                else
                {
                    // Not a line we know how to parse
                    continue;
                }
            }

            if (lineParsers.TryGetValue(prefix, out Func<string, LogLine>? parser))
            {
                yield return parser(line);
            }
        }
    }

    public static DiscInfo Organize(string inputPath)
    {
        if (File.Exists(inputPath))
        {
            return Organize(Parse(File.ReadAllLines(inputPath)));
        }

        return new DiscInfo();
    }

    public static DiscInfo Organize(IEnumerable<LogLine> lines)
    {
        var disc = new DiscInfo();

        Title? currentTrack = null;
        Segment? currentSegment = null;
        int expectedTrackCount = 0;

        foreach (var line in lines)
        {
            switch (line)
            {
                case TrackCountLogLine source:
                    expectedTrackCount = source.Count;
                    break;
                case SourceInformationLogLine source:
                    switch (source.Code)
                    {
                        case DiscInfo.TypeId:
                            disc.Type = source.Message;
                            break;
                        case DiscInfo.NameId:
                            disc.Name = source.Message;
                            break;
                        case Segment.LanguageCodeId:
                            disc.LanguageCode = source.Message;
                            break;
                        case Segment.LanguageId:
                            disc.Language = source.Message;
                            break;
                    }
                    break;
                case TrackInformationLogLine track:
                    if (currentSegment != null)
                    {
                        // the current segment is done and we are starting a new track
                        if (currentTrack == null)
                        {
                            throw new Exception("Cannot start a new track when the previous track was never started");
                        }

                        currentTrack.Segments.Add(currentSegment);
                        currentSegment = null;

                        disc.Titles.Add(currentTrack);
                        currentTrack = null;
                    }

                    if (currentTrack == null)
                    {
                        // no track has been started yet
                        currentTrack = new Title
                        {
                            Index = track.Index
                        };
                    }

                    switch (track.Code)
                    {
                        case Title.ChapterCountId:
                            if (Int32.TryParse(track.Message, out int val))
                            {
                                currentTrack.ChapterCount = val;
                            }
                            break;
                        case Title.SizeId:
                            if (long.TryParse(track.Message, out long result))
                            {
                                currentTrack.Size = result;
                            }
                            break;
                        case Title.DisplaySizeId:
                            currentTrack.DisplaySize = track.Message;
                            break;
                        case Title.PlaylistId:
                            currentTrack.Playlist = track.Message;
                            break;
                        case Title.SegmentMapId:
                            currentTrack.SegmentMap = track.Message;
                            break;
                        case Title.CommentId:
                            currentTrack.Comment = track.Message;
                            break;
                        case Title.LengthId:
                            currentTrack.Length = track.Message;
                            break;
                        case Title.SourceTitleId:
                            currentTrack.Playlist = track.Message;
                            break;
                    }
                    break;
                case SegmentInformationLogLine segment:
                    switch (segment.Code)
                    {
                        case Segment.TypeId: // Also the first entry
                            if (currentSegment != null && currentTrack != null)
                            {
                                currentTrack.Segments.Add(currentSegment);
                            }

                            currentSegment = new Segment
                            {
                                Index = segment.SegmentIndex,
                                Type = segment.Message
                            };
                            break;
                        case Segment.NameId:
                            if (currentSegment != null)
                            {
                                currentSegment.Name = segment.Message;
                            }
                            break;
                        case Segment.AudioTypeId:
                            if (currentSegment != null)
                            {
                                currentSegment.AudioType = segment.Message;
                            }
                            break;
                        case Segment.LanguageCodeId:
                            if (currentSegment != null)
                            {
                                currentSegment.LanguageCode = segment.Message;
                            }
                            break;
                        case Segment.LanguageId:
                            if (currentSegment != null)
                            {
                                currentSegment.Language = segment.Message;
                            }
                            break;
                        case Segment.ResolutionId:
                            if (currentSegment != null)
                            {
                                currentSegment.Resolution = segment.Message;
                            }
                            break;
                        case Segment.AspectRatioId:
                            if (currentSegment != null)
                            {
                                currentSegment.AspectRatio = segment.Message;
                            }
                            break;
                    }

                    break;
                case HashInfoLogLine hash:
                    disc.HashInfo.Add(hash);
                    break;
            }
        }

        // finish up the last segment and track
        if (currentSegment == null)
        {
            throw new Exception("Unexpected end of disc with no current segment");
        }

        if (currentTrack == null)
        {
            throw new Exception("Unexpected end of disc with no current track");
        }

        currentTrack.Segments.Add(currentSegment);
        disc.Titles.Add(currentTrack);

        return disc;
    }
}
