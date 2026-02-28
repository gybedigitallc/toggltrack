using System.Text.Json;

public static class SecretsLoader
{
    public static string Parse(string json)
    {
        var root = JsonSerializer.Deserialize<RootDto>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to parse secrets JSON.");

        return root.Secrets.ApiToken;
    }

    public static string Load(string path) =>
        Parse(File.ReadAllText(path));

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private record SecretsDto(string ApiToken);
    private record RootDto(SecretsDto Secrets);
}
