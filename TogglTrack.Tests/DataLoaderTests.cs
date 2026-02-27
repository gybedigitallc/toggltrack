using Xunit;

public class DataLoaderTests
{
    private const string SampleJson = """
        {
            "clients": [
                {
                    "client": "GybeDigital",
                    "projects": {
                        "Marina": 217165158,
                        "Training": 217165358
                    }
                },
                {
                    "client": "BISSELL",
                    "projects": {
                        "IOMS": 217165342,
                        "KTLO": 217165346
                    }
                }
            ]
        }
        """;

    // Cycle 1: Parse returns client names
    [Fact]
    public void Parse_ReturnsClientNames()
    {
        var clients = DataLoader.Parse(SampleJson);

        Assert.Equal(2, clients.Count);
        Assert.Equal("GybeDigital", clients[0].Name);
        Assert.Equal("BISSELL", clients[1].Name);
    }

    // Cycle 2: Parse returns projects for each client
    [Fact]
    public void Parse_ReturnsProjectsForClient()
    {
        var clients = DataLoader.Parse(SampleJson);
        var gybe = clients[0];

        Assert.Equal(2, gybe.Projects.Count);
        Assert.Equal(217165158, gybe.Projects["Marina"]);
        Assert.Equal(217165358, gybe.Projects["Training"]);
    }

    // Cycle 3: Parse handles both clients correctly
    [Fact]
    public void Parse_HandlesBothClients()
    {
        var clients = DataLoader.Parse(SampleJson);
        var bissell = clients[1];

        Assert.Equal(2, bissell.Projects.Count);
        Assert.Equal(217165342, bissell.Projects["IOMS"]);
        Assert.Equal(217165346, bissell.Projects["KTLO"]);
    }
}
