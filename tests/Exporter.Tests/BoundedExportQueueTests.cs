using TiaFileFormatExporter.Classes;

namespace Exporter.Tests;

[TestClass]
public class BoundedExportQueueTests
{
    [TestMethod]
    public async Task BackpressureBoundsQueuedWorkAndEveryJobRunsOnce()
    {
        await using var queue = new BoundedExportQueue(2);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var active = 0;
        var maximum = 0;
        var completed = new int[1000];
        var producer = Task.Run(async () =>
        {
            for (var i = 0; i < completed.Length; i++)
            {
                var index = i;
                await queue.EnqueueAsync(async () =>
                {
                    var current = Interlocked.Increment(ref active);
                    if (current == 2)
                        started.TrySetResult();
                    InterlockedExtensions.Max(ref maximum, current);
                    await release.Task;
                    Interlocked.Increment(ref completed[index]);
                    Interlocked.Decrement(ref active);
                });
            }
        });
        try
        {
            await started.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.IsFalse(producer.IsCompleted, "The producer must wait for bounded queue capacity.");
            Assert.IsLessThanOrEqualTo(6L, queue.SubmittedCount);
        }
        finally { release.TrySetResult(); }
        await producer.WaitAsync(TimeSpan.FromSeconds(10));
        await queue.CompleteAsync().WaitAsync(TimeSpan.FromSeconds(10));
        Assert.AreEqual(2, maximum);
        Assert.IsTrue(completed.All(x => x == 1));
        Assert.AreEqual(1000L, queue.SubmittedCount);
    }

    [TestMethod]
    public async Task DrainSupportsSeparateImageAndObjectPhases()
    {
        await using var queue = new BoundedExportQueue(2);
        await queue.DrainAsync();
        var phase = 0;
        await queue.EnqueueAsync(async () => { await Task.Yield(); phase = 1; });
        await queue.DrainAsync().WaitAsync(TimeSpan.FromSeconds(10));
        Assert.AreEqual(1, phase);
        await queue.EnqueueAsync(() => { phase = 2; return Task.CompletedTask; });
        await queue.CompleteAsync().WaitAsync(TimeSpan.FromSeconds(10));
        Assert.AreEqual(2, phase);
        await Assert.ThrowsExactlyAsync<System.Threading.Channels.ChannelClosedException>(async () =>
            await queue.EnqueueAsync(() => Task.CompletedTask));
    }

    [TestMethod]
    public async Task FailedJobDoesNotStrandProducerOrRemainingWork()
    {
        var queue = new BoundedExportQueue(1);
        var completed = 0;
        for (var i = 0; i < 20; i++)
            await queue.EnqueueAsync(() =>
            {
                if (Interlocked.Increment(ref completed) == 1)
                    throw new InvalidOperationException("test failure");
                return Task.CompletedTask;
            });
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
            queue.CompleteAsync().WaitAsync(TimeSpan.FromSeconds(10)));
        Assert.AreEqual(20, completed);
    }

    private static class InterlockedExtensions
    {
        public static void Max(ref int location, int value)
        {
            var previous = Volatile.Read(ref location);
            while (previous < value)
            {
                var actual = Interlocked.CompareExchange(ref location, value, previous);
                if (actual == previous)
                    return;
                previous = actual;
            }
        }
    }
}
