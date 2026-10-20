using Microsoft.VisualStudio.TestTools.UnitTesting;
using TiaFileFormatExporter.Classes;

namespace Exporter.Tests;

[TestClass]
public class PriorityExportSchedulerTests
{
    [TestMethod]
    public async Task ExpensiveWorkStartsFirstAndEqualCostsKeepTraversalOrder()
    {
        var scheduler = new PriorityExportScheduler();
        var order = new List<int>();
        foreach (var item in new[] { (Id: 0, Cost: 1L), (Id: 1, Cost: 100L), (Id: 2, Cost: 100L), (Id: 3, Cost: 2L) })
            scheduler.Add(() => { order.Add(item.Id); return Task.CompletedTask; }, item.Cost);
        await using var queue = new BoundedExportQueue(1);
        await scheduler.DispatchAsync(queue);
        await queue.CompleteAsync();
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 0 }, order);
    }
}
