using System.Net.Http.Json;
using System.Text.Json.Serialization;

public record TogglUser(int WorkspaceId);
public record TogglTimeEntry(long Id);

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

    public async Task<TogglTimeEntry> CreateTimeEntry(int workspaceId, int projectId, DateTimeOffset start, TimeSpan duration, string? description = null)
    {
        var stop = start + duration;
        object body = description != null
            ? new { workspace_id = workspaceId, project_id = projectId, start = start.ToString("O"), stop = stop.ToString("O"), duration = (int)duration.TotalSeconds, created_with = "TogglTrack", description }
            : new { workspace_id = workspaceId, project_id = projectId, start = start.ToString("O"), stop = stop.ToString("O"), duration = (int)duration.TotalSeconds, created_with = "TogglTrack" };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/workspaces/{workspaceId}/time_entries");
        AddAuth(request);
        request.Content = JsonContent.Create(body, body.GetType());

        var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<TimeEntryDto>()
            ?? throw new InvalidOperationException("Failed to deserialize time entry.");

        return new TogglTimeEntry(dto.Id);
    }

    private void AddAuth(HttpRequestMessage request)
    {
        var credentials = Convert.ToBase64String(
            System.Text.Encoding.ASCII.GetBytes($"{apiToken}:api_token"));
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
    }

    private record UserDto([property: JsonPropertyName("default_workspace_id")] int DefaultWorkspaceId);
    private record TimeEntryDto([property: JsonPropertyName("id")] long Id);
}
