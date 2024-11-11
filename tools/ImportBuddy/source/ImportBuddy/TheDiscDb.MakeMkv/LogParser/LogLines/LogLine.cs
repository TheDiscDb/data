namespace MakeMkv
{
    using System;

    [System.Diagnostics.DebuggerDisplay("{OriginalLine}")]
    public abstract class LogLine
    {
        public string? OriginalLine { get; protected set; }

        public LogLine(string prefix)
        {
            this.Prefix = prefix;
        }

        public string? Prefix { get; set; }

        public bool Matches(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return false;
            }

            return this.Prefix != null && this.Prefix.Equals(prefix, StringComparison.OrdinalIgnoreCase);
        }

        protected static int TryParseInt(int index, string[] parts)
        {
            if (index < parts.Length)
            {
                if (Int32.TryParse(parts[index], out int val))
                {
                    return val;
                }
            }

            return default(int);
        }

        protected static int TryParseInt(CsvEnumerator enumerator)
        {
            if (enumerator.MoveNext())
            {
                if (Int32.TryParse(enumerator.Current, out int val))
                {
                    return val;
                }
            }

            return default;
        }

        protected static bool TryParseBoolean(int index, string[] parts)
        {
            if (index < parts.Length)
            {
                if (Int32.TryParse(parts[index], out int val))
                {
                    return val != 256 && val != 999 && val > 0;
                }
            }

            return default(bool);
        }

        protected static bool TryParseBoolean(CsvEnumerator enumerator)
        {
            if (enumerator.MoveNext())
            {
                if (Int32.TryParse(enumerator.Current, out int val))
                {
                    return val != 256 && val != 999 && val > 0;
                }
            }

            return default;
        }

        protected static string? GetString(int index, string[] parts)
        {
            if (index < parts.Length)
            {
                return parts[index].Replace("\"", string.Empty);
            }

            return default;
        }

        protected static string? GetString(CsvEnumerator enumerator)
        {
            if (enumerator.MoveNext())
                return enumerator.Current.ToString().Replace("\"", string.Empty);

            return default;
        }

        protected static DateTime TryParseDateTime(int index, string[] parts)
        {
            if (index < parts.Length)
            {
                if (DateTime.TryParse(parts[index], out DateTime val))
                {
                    return val;
                }
            }

            return default(DateTime);
        }

        protected static DateTime TryParseDateTime(CsvEnumerator enumerator)
        {
            if (enumerator.MoveNext())
            {
                if (DateTime.TryParse(enumerator.Current, out DateTime val))
                {
                    return val;
                }
            }

            return default;
        }

        protected static long TryParseLong(int index, string[] parts)
        {
            if (index < parts.Length)
            {
                if (Int64.TryParse(parts[index], out long val))
                {
                    return val;
                }
            }

            return default(long);
        }

        protected static long TryParseLong(CsvEnumerator enumerator)
        {
            if (enumerator.MoveNext())
            {
                if (Int64.TryParse(enumerator.Current, out long val))
                {
                    return val;
                }
            }

            return default;
        }
    }
}
