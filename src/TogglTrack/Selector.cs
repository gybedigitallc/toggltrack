using Spectre.Console;

public static class Selector
{
    private const string BackSentinel = "Back";
    private const string QuitSentinel = "Quit";

    public static (string ProjectName, int ProjectId)? Run(IReadOnlyList<Client> clients, IAnsiConsole? console = null)
    {
        var c = console ?? AnsiConsole.Console;
        return Run(
            clients,
            clients => PromptClient(clients, c),
            client => PromptProject(client, c),
            onError: ex => c.WriteLine($"Error: {ex.Message}"));
    }

    public static (string ProjectName, int ProjectId)? Run(
        IReadOnlyList<Client> clients,
        Func<IReadOnlyList<Client>, Client?> clientSelector,
        Func<Client, string?> projectSelector,
        Action<Exception>? onError = null)
    {
        while (true)
        {
            Client? client;
            try { client = clientSelector(clients); }
            catch (Exception ex) { onError?.Invoke(ex); return null; }

            if (client is null) return null;

            string? projectName;
            try { projectName = projectSelector(client); }
            catch (Exception ex) { onError?.Invoke(ex); return null; }

            if (projectName is null) continue;

            return (projectName, client.Projects[projectName]);
        }
    }

    private static Client? PromptClient(IReadOnlyList<Client> clients, IAnsiConsole console)
    {
        var choice = console.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a client:")
                .AddChoices(clients.Select(c => c.Name).Append(QuitSentinel)));
        return choice == QuitSentinel ? null : clients.First(c => c.Name == choice);
    }

    private static string? PromptProject(Client client, IAnsiConsole console)
    {
        var choice = console.Prompt(
            new SelectionPrompt<string>()
                .Title($"Select a project for {client.Name}:")
                .AddChoices(client.Projects.Keys.Append(BackSentinel)));
        return choice == BackSentinel ? null : choice;
    }
}
