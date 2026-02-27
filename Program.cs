using Spectre.Console;

AnsiConsole.Profile.Capabilities.Interactive = true;
var clients = DataLoader.Load("data.json");
var client = Selector.SelectClient(clients);
var (_, projectId) = Selector.SelectProject(client);
Console.WriteLine(projectId);
