using Spectre.Console;

namespace ImportBuddy;

public class Shell
{
    private readonly Dictionary<string, IConsoleTask> tasks = new();

    public Shell(IEnumerable<IConsoleTask> tasks)
    {
        this.tasks = tasks
            .OrderBy(t => t.Id)
            .ToDictionary(t => t.Id.ToString());
    }

    public async Task RunAsync(CancellationTokenSource cancellationTokenSource)
    {
        var menu = this.tasks.Select(t => t.Value.MenuText);

        SelectionPrompt<IConsoleTask> choices = new SelectionPrompt<IConsoleTask>()
            .AddChoices(this.tasks.Values);

        choices.Converter = t => t.MenuText;

        IConsoleTask choice;
        do
        {
            choice = AnsiConsole.Prompt(choices);
            await choice.ExecuteAsync(cancellationTokenSource.Token);
        } while (choice.Id != ushort.MaxValue);
    }
}
