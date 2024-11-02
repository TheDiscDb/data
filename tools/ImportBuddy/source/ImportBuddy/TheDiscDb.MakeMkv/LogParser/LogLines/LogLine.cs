namespace MakeMkv
{
    using System;

    [System.Diagnostics.DebuggerDisplay("{OriginalLine}")]
    public abstract class LogLine
    {
        public string OriginalLine { get; protected set; }

        public LogLine(string prefix)
        {
            this.Prefix = prefix;
        }

        public string Prefix { get; set; }

        public bool Matches(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return false;
            }

            return this.Prefix.Equals(prefix, StringComparison.OrdinalIgnoreCase);
        }

        protected static int TryParseInt(int index, string[] parts)
        {
            if (index < parts.Length)
            {
                if(Int32.TryParse(parts[index], out int val))
                {
                    return val;
                }
            }

            return default(int);
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

        protected static string GetString(int index, string[] parts)
        {
            if (index < parts.Length)
            {
                return parts[index].Replace("\"", string.Empty);
            }

            return default(string);
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
    }
}
