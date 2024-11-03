namespace TheDiscDb.InputModels;

public interface IPageInfo
{
    public bool HasNextPage { get; }

    public bool HasPreviousPage { get; }

    public string? StartCursor { get; }

    public string? EndCursor { get; }
}