public static class ProgramRunner
{
    public static async Task RunAsync(
        Func<(string ProjectName, int ProjectId)?> select,
        ITogglClient toggl,
        Func<string?> descriptionPrompt,
        Action<string> output,
        int workspaceId)
    {
        var selection = select();
        if (selection is null) return;
        var (_, projectId) = selection.Value;

        var description = descriptionPrompt();
        var entry = await toggl.CreateTimeEntry(
            workspaceId,
            projectId,
            DateTimeOffset.UtcNow - TimeSpan.FromMinutes(25),
            TimeSpan.FromMinutes(25),
            description);
        output($"Created time entry {entry.Id}");
    }
}
