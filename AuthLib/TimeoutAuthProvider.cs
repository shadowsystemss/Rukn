
namespace AuthLib
{
    public class TimeoutAuthProvider(IAuthProvider authProvider, int milliseconds) : IAuthProvider
    {
        public async Task<bool> TestAsync(string UID, CancellationToken cancel = default)
        {
            Task<bool> task = authProvider.TestAsync(UID, cancel);
            await Task.WhenAny(task, Task.Delay(milliseconds, cancel));

            return task.IsCompletedSuccessfully && task.Result;
        }
    }
}
