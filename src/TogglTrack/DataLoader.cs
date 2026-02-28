using System.Text.Json;
using System.Text.Json.Serialization;

public static class DataLoader
{
    public static AppData Parse(string json)
    {
        var root = JsonSerializer.Deserialize<RootDto>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to parse JSON.");

        var clients = root.Clients
            .Select(c => new Client(c.Client, c.Projects))
            .ToList();

        return new AppData(root.WorkspaceId, clients);
    }

    public static AppData Load(string path) =>
        Parse(File.ReadAllText(path));

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private record RootDto(int WorkspaceId, List<ClientDto> Clients);
    private record ClientDto(string Client, Dictionary<string, int> Projects);
}
