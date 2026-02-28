using Spectre.Console;

AnsiConsole.Profile.Capabilities.Interactive = true;
AnsiConsole.Profile.Capabilities.Ansi = true;
var data = DataLoader.Load("data.json");
var apiToken = SecretsLoader.Load("secrets.json");
var toggl = new TogglClient(new System.Net.Http.HttpClient(), apiToken);

await ProgramRunner.RunAsync(
    select: () => Selector.Run(data.Clients),
    toggl: toggl,
    descriptionPrompt: () =>
    {
        var input = AnsiConsole.Prompt(
            new TextPrompt<string>("Description [grey](optional, press Enter to skip)[/]:")
                .AllowEmpty());
        return string.IsNullOrEmpty(input) ? null : input;
    },
    output: Console.WriteLine,
    workspaceId: data.WorkspaceId);
