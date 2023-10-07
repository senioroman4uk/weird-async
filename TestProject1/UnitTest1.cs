using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace TestProject1;

public class UnitTest1
{
    private TimeSpan operationTimeout = TimeSpan.FromMilliseconds(20);
    
    [Fact]
    public async Task CallCancelAfterTaskDelay()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var timeoutCts = new CancellationTokenSource();
        
        var combined = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, timeoutCts.Token);
        var timeoutTask = timeoutCts.Token.AsTask();
       
        var actionTask = ActionAsync(combined.Token);
        var resultTask = Task.WhenAny(actionTask, timeoutTask).ConfigureAwait(false);
        await Task.Delay(operationTimeout);
        
        // Cancel will wait for all of the callbacks assigned to cancellation token source to finish
        timeoutCts.Cancel();
        var resultingTask = await resultTask;

        resultingTask.Should().Be(timeoutTask);
        combined.IsCancellationRequested.Should().Be(true);
    }

    [Fact]
    public async Task CancelAfter()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var timeoutCts = new CancellationTokenSource();
        
        var combined = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, timeoutCts.Token);
        var timeoutTask = timeoutCts.Token.AsTask();
       
        var actionTask = ActionAsync(combined.Token);
        var resultTask = Task.WhenAny(actionTask, timeoutTask).ConfigureAwait(false);
        timeoutCts.CancelAfter(operationTimeout);
        var resultingTask = await resultTask;

        resultingTask.Should().Be(timeoutTask);
        // we did not wait for all of the callbacks to finish
        combined.IsCancellationRequested.Should().Be(false);
    }
    
    private async Task ActionAsync(CancellationToken cancellationToken)
    {
        // Emulate long action
        await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
            
        Console.WriteLine("I am here");

        // code should not reach here since task will be cancelled
        throw new Exception("boom");
    }
}