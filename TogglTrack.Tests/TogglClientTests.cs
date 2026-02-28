using System.Net;
using System.Net.Http;
using Xunit;

public class TogglClientTests
{
    // Cycle 2: GetCurrentUser returns a TogglUser with WorkspaceId
    [Fact]
    public async Task GetCurrentUser_ReturnsWorkspaceId()
    {
        var handler = new FakeHttpHandler("""{"default_workspace_id":5678}""");
        var client = new TogglClient(new HttpClient(handler), "any-token");

        var user = await client.GetCurrentUser();

        Assert.Equal(5678, user.WorkspaceId);
    }

    // CreateTimeEntry — Cycle 1: returns TogglTimeEntry with ID from response body
    [Fact]
    public async Task CreateTimeEntry_ReturnsIdFromResponse()
    {
        var handler = new FakeHttpHandler("""{"id":9999}""");
        var client = new TogglClient(new HttpClient(handler), "any-token");

        var entry = await client.CreateTimeEntry(42, 7, DateTimeOffset.UtcNow, TimeSpan.FromMinutes(25));

        Assert.Equal(9999, entry.Id);
    }

    // CreateTimeEntry — Cycle 2: sends POST to correct URL
    [Fact]
    public async Task CreateTimeEntry_SendsPostToCorrectUrl()
    {
        var handler = new CapturingHttpHandler("""{"id":0}""");
        var client = new TogglClient(new HttpClient(handler), "any-token");

        await client.CreateTimeEntry(42, 7, DateTimeOffset.UtcNow, TimeSpan.FromMinutes(25));

        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.EndsWith("/workspaces/42/time_entries", handler.LastRequest.RequestUri!.AbsolutePath);
    }

    // CreateTimeEntry — Cycle 3: sends correct JSON body
    [Fact]
    public async Task CreateTimeEntry_SendsCorrectJsonBody()
    {
        var handler = new CapturingHttpHandler("""{"id":0}""");
        var client = new TogglClient(new HttpClient(handler), "any-token");
        var start = new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero);
        var duration = TimeSpan.FromMinutes(25);

        await client.CreateTimeEntry(42, 7, start, duration);

        var json = System.Text.Json.JsonDocument.Parse(handler.LastRequestBody!).RootElement;
        Assert.Equal(42, json.GetProperty("workspace_id").GetInt32());
        Assert.Equal(7, json.GetProperty("project_id").GetInt32());
        Assert.Equal(start.ToString("O"), json.GetProperty("start").GetString());
        Assert.Equal((start + duration).ToString("O"), json.GetProperty("stop").GetString());
        Assert.Equal(1500, json.GetProperty("duration").GetInt32());
    }

    // Description — Cycle 1: with description, body includes "description" field
    [Fact]
    public async Task CreateTimeEntry_WithDescription_SendsDescriptionInBody()
    {
        var handler = new CapturingHttpHandler("""{"id":0}""");
        var client = new TogglClient(new HttpClient(handler), "any-token");

        await client.CreateTimeEntry(42, 7, DateTimeOffset.UtcNow, TimeSpan.FromMinutes(25), "my task");

        var json = System.Text.Json.JsonDocument.Parse(handler.LastRequestBody!).RootElement;
        Assert.Equal("my task", json.GetProperty("description").GetString());
    }

    // Description — Cycle 2: without description, body omits "description" field
    [Fact]
    public async Task CreateTimeEntry_WithoutDescription_OmitsDescriptionFromBody()
    {
        var handler = new CapturingHttpHandler("""{"id":0}""");
        var client = new TogglClient(new HttpClient(handler), "any-token");

        await client.CreateTimeEntry(42, 7, DateTimeOffset.UtcNow, TimeSpan.FromMinutes(25));

        var json = System.Text.Json.JsonDocument.Parse(handler.LastRequestBody!).RootElement;
        Assert.False(json.TryGetProperty("description", out _));
    }

    // Cycle 3: GetCurrentUser sends correct Basic Auth header
    [Fact]
    public async Task GetCurrentUser_SendsBasicAuthHeader()
    {
        var handler = new CapturingHttpHandler("""{"default_workspace_id":0}""");
        var client = new TogglClient(new HttpClient(handler), "mytoken");

        await client.GetCurrentUser();

        var auth = handler.LastRequest!.Headers.Authorization!;
        Assert.Equal("Basic", auth.Scheme);
        var decoded = System.Text.Encoding.ASCII.GetString(Convert.FromBase64String(auth.Parameter!));
        Assert.Equal("mytoken:api_token", decoded);
    }
}

file sealed class FakeHttpHandler(string responseBody) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken) =>
        Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseBody)
        });
}

file sealed class CapturingHttpHandler(string responseBody) : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }
    public string? LastRequestBody { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        if (request.Content != null)
            LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseBody)
        };
    }
}
