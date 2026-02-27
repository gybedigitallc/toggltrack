using System.Text.Json;
using System.Text.Json.Serialization;

public static class DataLoader
{
    public static IReadOnlyList<Client> Parse(string json)
    {
        var root = JsonSerializer.Deserialize<RootDto>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to parse JSON.");

        return root.Clients
            .Select(c => new Client(c.Client, c.Projects))
            .ToList();
    }

    public static IReadOnlyList<Client> Load(string path) =>
        Parse(File.ReadAllText(path));

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private record RootDto(List<ClientDto> Clients);
    private record ClientDto(string Client, Dictionary<string, int> Projects);
}
