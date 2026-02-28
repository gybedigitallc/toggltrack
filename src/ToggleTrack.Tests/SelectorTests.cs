using Spectre.Console.Testing;
using Xunit;

namespace TogglTrack.Tests;

public class SelectorTests
{
    private static readonly List<Client> OneClient =
    [
        new("Acme", new Dictionary<string, int> { { "Alpha", 1 } })
    ];

    // Cycle 1: happy path — selected project is returned
    [Fact]
    public void Run_ReturnsSelectedProject()
    {
        var result = Selector.Run(
            OneClient,
            clientSelector: clients => clients[0],
            projectSelector: _ => "Alpha");

        Assert.Equal(("Alpha", 1), result);
    }

    // Cycle 2: going back (null from projectSelector) re-prompts clientSelector
    [Fact]
    public void Run_WhenProjectSelectorReturnsNull_RepromptsClient()
    {
        int clientCalls = 0;
        int projectCalls = 0;

        var result = Selector.Run(
            OneClient,
            clientSelector: clients => { clientCalls++; return clients[0]; },
            projectSelector: _ => projectCalls++ == 0 ? null : "Alpha");

        Assert.Equal(("Alpha", 1), result);
        Assert.Equal(2, clientCalls);
    }

    // Cycle 3: quitting (null from clientSelector) → Run returns null
    [Fact]
    public void Run_WhenClientSelectorReturnsNull_ReturnsNull()
    {
        var result = Selector.Run(
            OneClient,
            clientSelector: _ => null,
            projectSelector: _ => "Alpha");

        Assert.Null(result);
    }

    // Cycle 4: after going back, a different client+project can be selected
    [Fact]
    public void Run_AfterGoingBack_CanSelectDifferentClient()
    {
        var clients = new List<Client>
        {
            new("Acme", new Dictionary<string, int> { { "Alpha", 1 } }),
            new("BetaCorp", new Dictionary<string, int> { { "Gamma", 2 } })
        };

        int attempt = 0;

        var result = Selector.Run(
            clients,
            clientSelector: c => c[attempt == 0 ? 0 : 1],
            projectSelector: _ => attempt++ == 0 ? null : "Gamma");

        Assert.Equal(("Gamma", 2), result);
    }

    // Error cycle 1: clientSelector throws → onError invoked, Run returns null
    [Fact]
    public void Run_WhenClientSelectorThrows_InvokesOnErrorAndReturnsNull()
    {
        var thrown = new InvalidOperationException("boom");
        Exception? captured = null;

        var result = Selector.Run(
            OneClient,
            clientSelector: _ => throw thrown,
            projectSelector: _ => "Alpha",
            onError: ex => captured = ex);

        Assert.Null(result);
        Assert.Same(thrown, captured);
    }

    // Error cycle 2: projectSelector throws → onError invoked, Run returns null
    [Fact]
    public void Run_WhenProjectSelectorThrows_InvokesOnErrorAndReturnsNull()
    {
        var thrown = new InvalidOperationException("boom");
        Exception? captured = null;

        var result = Selector.Run(
            OneClient,
            clientSelector: clients => clients[0],
            projectSelector: _ => throw thrown,
            onError: ex => captured = ex);

        Assert.Null(result);
        Assert.Same(thrown, captured);
    }

    // Console cycle 1: pressing Enter on first client and first project returns that project
    [Fact]
    public void Run_WithConsole_SelectsFirstClientAndProject()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Input.PushKey(ConsoleKey.Enter); // select first client (Acme)
        console.Input.PushKey(ConsoleKey.Enter); // select first project (Alpha)

        var result = Selector.Run(OneClient, console);

        Assert.Equal(("Alpha", 1), result);
    }

    // Console cycle 2: navigating to Quit returns null
    [Fact]
    public void Run_WithConsole_WhenQuitSelected_ReturnsNull()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Input.PushKey(ConsoleKey.DownArrow); // move from Acme to Quit
        console.Input.PushKey(ConsoleKey.Enter);     // select Quit

        var result = Selector.Run(OneClient, console);

        Assert.Null(result);
    }

    // Console cycle 3: selecting Back re-prompts client, then succeeds on second try
    [Fact]
    public void Run_WithConsole_WhenBackSelected_RepromptsClient()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Input.PushKey(ConsoleKey.Enter);     // select first client (Acme)
        console.Input.PushKey(ConsoleKey.DownArrow); // move from Alpha to Back
        console.Input.PushKey(ConsoleKey.Enter);     // select Back
        console.Input.PushKey(ConsoleKey.Enter);     // select first client (Acme) again
        console.Input.PushKey(ConsoleKey.Enter);     // select first project (Alpha)

        var result = Selector.Run(OneClient, console);

        Assert.Equal(("Alpha", 1), result);
    }
}
