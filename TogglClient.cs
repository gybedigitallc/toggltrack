using System.Net.Http.Json;
using System.Text.Json.Serialization;

public record TogglUser(int WorkspaceId);

public class TogglClient(HttpClient http, string apiToken)
{
    private const string BaseUrl = "https://api.track.toggl.com/api/v9";

    public async Task<TogglUser> GetCurrentUser()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/me");
        AddAuth(request);

        var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<UserDto>()
            ?? throw new InvalidOperationException("Failed to deserialize user.");

        return new TogglUser(dto.DefaultWorkspaceId);
    }

    private void AddAuth(HttpRequestMessage request)
    {
        var credentials = Convert.ToBase64String(
            System.Text.Encoding.ASCII.GetBytes($"{apiToken}:api_token"));
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
    }

    private record UserDto([property: JsonPropertyName("default_workspace_id")] int DefaultWorkspaceId);
}
