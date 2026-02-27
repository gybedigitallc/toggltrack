using Spectre.Console;

AnsiConsole.Profile.Capabilities.Interactive = true;
var clients = DataLoader.Load("data.json");
var client = Selector.SelectClient(clients);
var (_, projectId) = Selector.SelectProject(client);

var apiToken = SecretsLoader.Load("secrets.json");
var toggl = new TogglClient(new System.Net.Http.HttpClient(), apiToken);
var user = await toggl.GetCurrentUser();
Console.WriteLine($"Project ID: {projectId}, Workspace ID: {user.WorkspaceId}");
