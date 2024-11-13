namespace MakeMkv;

/// <summary>
/// Iterates over comma separated values in a single line of text. 
/// </summary>
public sealed class CsvEnumerator
{
    private readonly String _span;

    private readonly Boolean _isInitialized;

    private Int32 _currentStart;
    private Int32 _currentEnd;

    /// <inheritdoc cref="IEnumerator{T}.Current" />
    public ReadOnlySpan<Char> Current => _span.AsSpan()[_currentStart.._currentEnd];

    /// <summary>
    /// Creates a new <see cref="CsvEnumerator"/> over a span of characters.
    /// </summary>
    public CsvEnumerator(String span)
    {
        _isInitialized = true;
        _span = span;
        _currentStart = 0;
        _currentEnd = -1;
    }
    
    /// <inheritdoc cref="IEnumerator{T}.MoveNext" />
    public Boolean MoveNext()
    {
        Int32 start = _currentEnd + 1;
        if (!_isInitialized || start > _span.Length)
            return false;

        var slice = _span[start..];
        _currentStart = start;

        Boolean quoted = false;
        Boolean escaped = false;
        Int32 i;
        for (i = 0; i < slice.Length; i++)
        {
            Char curChar = slice[i];
            if (curChar == '\\')
            {
                if (!escaped)
                {
                    escaped = true;
                    continue;
                }
            }
            else if (curChar == '"')
            {
                if (!escaped)
                    quoted = !quoted;
            }
            else if (curChar == ',')
            {
                if (!quoted)
                    break;
            }

            escaped = false;
        }

        Int32 elementLength = i;

        _currentEnd = _currentStart + elementLength;
        return true;
    }

    /// <summary>
    /// Returns an enumerator suitable for use in foreach loops.
    /// </summary>
    public CsvEnumerator GetEnumerator() => this;

    /// <summary>
    /// Attempts to advance the enumerator and parse the value as an <see cref="int"/>.
    /// </summary>
    /// <returns>The parsed value, or the <see langword="default"/> value if enumeration or parsing failed.</returns>
    public int TryParseInt()
    {
        if (MoveNext())
        {
            if (Int32.TryParse(Current, out int val))
            {
                return val;
            }
        }

        return default;
    }

    /// <summary>
    /// Attempts to advance the enumerator and parse the value as a <see cref="bool"/>.
    /// </summary>
    /// <returns>The parsed value, or <see langword="false"/> if enumeration or parsing failed.</returns>
    public bool TryParseBoolean()
    {
        if (MoveNext())
        {
            if (Int32.TryParse(Current, out int val))
            {
                return val != 256 && val != 999 && val > 0;
            }
        }

        return default;
    }

    /// <summary>
    /// Attempts to advance the enumerator and parse the value as a <see cref="long"/>.
    /// </summary>
    /// <returns>The parsed value, or <see langword="null"/> if enumeration or parsing failed.</returns>
    public string? GetString()
    {
        if (MoveNext())
        {
            return Current.ToString().Replace("\"", string.Empty);
        }

        return default;
    }

    /// <summary>
    /// Attempts to advance the enumerator and parse the value as a <see cref="DateTime"/>.
    /// </summary>
    /// <returns>The parsed value, or the <see langword="default"/> value if enumeration or parsing failed.</returns>
    public DateTime TryParseDateTime()
    {
        if (MoveNext())
        {
            if (DateTime.TryParse(Current, out DateTime val))
            {
                return val;
            }
        }

        return default;
    }

    /// <summary>
    /// Attempts to advance the enumerator and parse the value as a <see cref="long"/>.
    /// </summary>
    /// <returns>The parsed value, or the <see langword="default"/> value if enumeration or parsing failed.</returns>
    public long TryParseLong()
    {
        if (MoveNext())
        {
            if (Int64.TryParse(Current, out long val))
            {
                return val;
            }
        }

        return default;
    }
}
