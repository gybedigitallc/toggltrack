using Spectre.Console;

AnsiConsole.Profile.Capabilities.Interactive = true;
var clients = DataLoader.Load("data.json");
var client = Selector.SelectClient(clients);
var (_, projectId) = Selector.SelectProject(client);

var apiToken = SecretsLoader.Load("secrets.json");
var toggl = new TogglClient(new System.Net.Http.HttpClient(), apiToken);
var user = await toggl.GetCurrentUser();

var descriptionInput = AnsiConsole.Prompt(
    new TextPrompt<string>("Description [grey](optional, press Enter to skip)[/]:")
        .AllowEmpty());
var description = string.IsNullOrEmpty(descriptionInput) ? null : descriptionInput;

var entry = await toggl.CreateTimeEntry(user.WorkspaceId, projectId, DateTimeOffset.UtcNow - TimeSpan.FromMinutes(25), TimeSpan.FromMinutes(25), description);
Console.WriteLine($"Created time entry {entry.Id}");
