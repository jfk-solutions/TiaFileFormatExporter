namespace TiaFileFormatExporter.Classes;

// Retain lightweight work descriptions, never converted models or output buffers.
internal sealed class PriorityExportScheduler
{
    private readonly List<(Func<Task> Work, long Cost)> pending = new();

    public void Add(Func<Task> work, long cost) => pending.Add((work, cost));

    public async Task DispatchAsync(BoundedExportQueue queue)
    {
        // OrderBy is stable: equally expensive items keep their traversal order.
        foreach (var item in pending.OrderByDescending(item => item.Cost))
            await queue.EnqueueAsync(item.Work);
        pending.Clear();
    }
}
