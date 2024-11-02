namespace TheDiscDb.Imdb;

public class KeyValueItem
{
    public string Key { get; set; }
    public string Value { get; set; }

    public KeyValueItem()
        : this("", "")
    {
    }

    public KeyValueItem(string key)
        : this(key, key)
    {
    }

    public KeyValueItem(string key, string value)
    {
        Key = key;
        Value = value;
    }
}
