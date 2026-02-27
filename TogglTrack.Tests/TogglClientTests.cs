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

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseBody)
        });
    }
}
