using Spectre.Console;

public static class Selector
{
    public static Client SelectClient(IReadOnlyList<Client> clients)
    {
        var clientName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a client:")
                .AddChoices(clients.Select(c => c.Name)));
        return clients.First(c => c.Name == clientName);
    }

    public static (string ProjectName, int ProjectId) SelectProject(Client client)
    {
        var projectName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"Select a project for {client.Name}:")
                .AddChoices(client.Projects.Keys));
        return (projectName, client.Projects[projectName]);
    }
}
