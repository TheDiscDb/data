namespace ImportBuddy;

public class ExitTask : IConsoleTask
{
    public ushort Id => ushort.MaxValue;
    public string MenuText => "Exit";
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
