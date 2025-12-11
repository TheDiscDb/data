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

    public static async Task CleanInPlace(string inputPath)
    {
        var lines = await File.ReadAllLinesAsync(inputPath);
        var cleanedLines = CleanLogs(lines, out bool fileChanged);
        if (fileChanged)
        {
            await File.WriteAllLinesAsync(inputPath, cleanedLines);
        }
    }

    public static IEnumerable<string> CleanLogs(IEnumerable<string> lines, out bool fileChanged)
    {
        List<string> output = new();
        fileChanged = false;
        foreach (var originalLine in lines)
        {
            // Remove empty lines from the output
            if (originalLine == string.Empty)
            {
                fileChanged = true;
                continue;
            }

            string? line = null;
            if (originalLine.StartsWith("MSG"))
            {
                var msgLine = MessageLogLine.Parse(originalLine);
                var (toRedact, replacement) = msgLine.Code switch
                {
                    // The debug file path is usually in the user's home directory
                    "1004" => (msgLine.Params[0], "***"),
                    // The drive name can sometimes contain a serial number
                    "2003" => (msgLine.Params[1], "***"),
                    // The .MakeMKV folder is usually in the user's home directory
                    "3338" => (msgLine.Params[1], "***"),
                    // No other messages are known to contain sensitive information
                    _ => (null, ""),
                };

                if (toRedact is not null)
                {
                    line = originalLine.Replace(toRedact, replacement);
                }
            }
            else if (originalLine.StartsWith("DRV"))
            {
                // DRV:1,2,999,12,"BD-ROM HL-DT-ST BDDVDRW UH12NS30 1.03","42","E:"
                // becomes
                // DRV:1,2,999,12,"***","42","***"
                var driveLine = DriveScanLogLine.Parse(originalLine);
                if (!string.IsNullOrEmpty(driveLine.DriveName))
                {
                    // Always redact the drive name, since it could contain a serial number
                    line = originalLine.Replace(driveLine.DriveName, "***");

                    if (!string.IsNullOrEmpty(driveLine.DiscName))
                    {
                        line = line.Replace(driveLine.DiscName, "***");
                    }

                    // Always redact the drive letter or path
                    if (!string.IsNullOrEmpty(driveLine.DriveLetter))
                    {
                        line = line.Replace(driveLine.DriveLetter, "***");
                    }
                }
            }

            if (line is null)
            {
                // This line wasn't modified, use the original line
                line = originalLine;
            }
            else
            {
                // Something was changed, ensure the file is overwritten
                fileChanged = true;
            }

            output.Add(line);
        }

        return output;
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
                        case Title.JavaCommentId:
                            currentTrack.JavaComment = track.Message;
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
