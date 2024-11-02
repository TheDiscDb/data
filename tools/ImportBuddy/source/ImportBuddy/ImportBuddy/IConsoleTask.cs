namespace ImportBuddy;

public interface IConsoleTask
{
    ushort Id { get; }
    string MenuText { get; }
    Task ExecuteAsync(CancellationToken cancellationToken);
}
