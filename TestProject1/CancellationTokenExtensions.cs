using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestProject1;

internal static class CancellationTokenExtensions
{
    
    public static Task AsTask(this CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource();
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        // A generalised version of this method would include a hotpath returning a canceled task (rather than setting up a registration) if (cancellationToken.IsCancellationRequested) on entry.  This is omitted, since we only start the timeout countdown in the token _after calling this method.

        IDisposable? registration = null;
        registration = cancellationToken.Register(() =>
        {
            tcs.TrySetCanceled();
            registration?.Dispose();
        }, useSynchronizationContext: false);

        return tcs.Task;
    }
}