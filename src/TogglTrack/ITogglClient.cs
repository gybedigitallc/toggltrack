public interface ITogglClient
{
    Task<TogglTimeEntry> CreateTimeEntry(int workspaceId, int projectId, DateTimeOffset start, TimeSpan duration, string? description = null);
}
