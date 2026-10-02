using System.Threading.Channels;

namespace TiaFileFormatExporter.Classes;

internal sealed class BoundedExportQueue : IAsyncDisposable
{
    private readonly Channel<Func<Task>> channel;
    private readonly Task[] workers;
    private readonly object stateLock = new();
    private TaskCompletionSource idle = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int pending;
    private long submitted;
    private Exception? failure;

    public BoundedExportQueue(int parallelism)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(parallelism);
        channel = Channel.CreateBounded<Func<Task>>(new BoundedChannelOptions(checked(parallelism * 2))
        {
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.Wait,
            AllowSynchronousContinuations = false
        });
        idle.SetResult();
        workers = Enumerable.Range(0, parallelism).Select(_ => Task.Run(WorkAsync)).ToArray();
    }

    public long SubmittedCount => Interlocked.Read(ref submitted);

    public async ValueTask EnqueueAsync(Func<Task> work)
    {
        lock (stateLock)
        {
            if (pending++ == 0)
                idle = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        try
        {
            await channel.Writer.WriteAsync(work);
            Interlocked.Increment(ref submitted);
        }
        catch
        {
            CompleteOne();
            throw;
        }
    }

    public async Task DrainAsync()
    {
        Task completion;
        lock (stateLock)
            completion = idle.Task;
        await completion;
        ThrowIfFailed();
    }

    public async Task CompleteAsync()
    {
        channel.Writer.TryComplete();
        await Task.WhenAll(workers);
        ThrowIfFailed();
    }

    public ValueTask DisposeAsync() => new(CompleteAsync());

    private async Task WorkAsync()
    {
        await foreach (var work in channel.Reader.ReadAllAsync())
        {
            try { await work(); }
            catch (Exception ex) { Interlocked.CompareExchange(ref failure, ex, null); }
            finally { CompleteOne(); }
        }
    }

    private void CompleteOne()
    {
        lock (stateLock)
            if (--pending == 0)
                idle.TrySetResult();
    }

    private void ThrowIfFailed()
    {
        if (failure != null)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
    }
}
