using Xunit;

namespace TogglTrack.Tests;

public class ProgramRunnerTests
{
    // Cycle 1: null selection → exits early, no API calls made
    [Fact]
    public async Task RunAsync_WhenSelectionIsNull_DoesNotCallToggl()
    {
        var toggl = new FakeTogglClient();

        await ProgramRunner.RunAsync(
            select: () => null,
            toggl: toggl,
            descriptionPrompt: () => null,
            output: _ => { },
            workspaceId: 1);

        Assert.False(toggl.CreateTimeEntryCalled);
    }

    // Cycle 2: valid selection → output contains the created entry ID
    [Fact]
    public async Task RunAsync_WhenSelectionIsValid_OutputsEntryId()
    {
        var toggl = new FakeTogglClient(entryId: 42);
        string? output = null;

        await ProgramRunner.RunAsync(
            select: () => ("Alpha", 1),
            toggl: toggl,
            descriptionPrompt: () => null,
            output: s => output = s,
            workspaceId: 10);

        Assert.Contains("42", output);
    }

    // Cycle 3: projectId from selection is forwarded to CreateTimeEntry
    [Fact]
    public async Task RunAsync_ForwardsProjectIdToCreateTimeEntry()
    {
        var toggl = new FakeTogglClient(entryId: 0);

        await ProgramRunner.RunAsync(
            select: () => ("Alpha", 7),
            toggl: toggl,
            descriptionPrompt: () => null,
            output: _ => { },
            workspaceId: 10);

        Assert.Equal(7, toggl.LastProjectId);
    }

    // Cycle 4: description from prompt is forwarded to CreateTimeEntry
    [Fact]
    public async Task RunAsync_ForwardsDescriptionToCreateTimeEntry()
    {
        var toggl = new FakeTogglClient(entryId: 0);

        await ProgramRunner.RunAsync(
            select: () => ("Alpha", 1),
            toggl: toggl,
            descriptionPrompt: () => "hello world",
            output: _ => { },
            workspaceId: 10);

        Assert.Equal("hello world", toggl.LastDescription);
    }

    // Cycle 5: workspaceId is forwarded to CreateTimeEntry
    [Fact]
    public async Task RunAsync_ForwardsWorkspaceIdToCreateTimeEntry()
    {
        var toggl = new FakeTogglClient(entryId: 0);

        await ProgramRunner.RunAsync(
            select: () => ("Alpha", 1),
            toggl: toggl,
            descriptionPrompt: () => null,
            output: _ => { },
            workspaceId: 99);

        Assert.Equal(99, toggl.LastWorkspaceId);
    }
}

file sealed class FakeTogglClient(long entryId = 0) : ITogglClient
{
    public bool CreateTimeEntryCalled { get; private set; }
    public int LastProjectId { get; private set; }
    public int LastWorkspaceId { get; private set; }
    public string? LastDescription { get; private set; }

    public Task<TogglTimeEntry> CreateTimeEntry(
        int workspaceId, int projectId, DateTimeOffset start, TimeSpan duration, string? description = null)
    {
        CreateTimeEntryCalled = true;
        LastWorkspaceId = workspaceId;
        LastProjectId = projectId;
        LastDescription = description;
        return Task.FromResult(new TogglTimeEntry(entryId));
    }
}
