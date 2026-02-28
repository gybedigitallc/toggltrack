using Xunit;

namespace TogglTrack.Tests;

public class SecretsLoaderTests
{
    private const string SampleJson = """
        {
            "secrets": {
                "apiToken": "test-token-abc"
            }
        }
        """;

    // Cycle 1: Parse returns the API token
    [Fact]
    public void Parse_ReturnsApiToken()
    {
        var token = SecretsLoader.Parse(SampleJson);

        Assert.Equal("test-token-abc", token);
    }
}
