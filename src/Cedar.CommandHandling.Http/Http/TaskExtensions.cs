// ReSharper disable once CheckNamespace
namespace System.Threading.Tasks
{
    using System.Runtime.CompilerServices;

    internal static class TaskExtensions
    {
        internal static ConfiguredTaskAwaitable NotOnCapturedContext(this Task task)
        {
            return task.ConfigureAwait(false);
        }

        internal static ConfiguredTaskAwaitable<TResult> NotOnCapturedContext<TResult>(this Task<TResult> task)
        {
            return task.ConfigureAwait(false);
        }

        internal static async Task WithTimeout(this Task task, TimeSpan delay)
        {
            var cts = new CancellationTokenSource();
            Task completedTask = await Task.WhenAny(task, Task.Delay(delay, cts.Token)); //.NotOnCapturedContext();
            if (completedTask != task)
            {
                throw new TimeoutException("The operation has timed out.");
            }
            cts.Cancel();
            await task.NotOnCapturedContext();
        }
    }
}
