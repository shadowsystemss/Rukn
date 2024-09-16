namespace AuthLib
{
    public class MultiAuthProvider : IAuthProvider
    {
        private List<IAuthProvider> _providers;
        public MultiAuthProvider(params IAuthProvider[] providers) => _providers = providers.ToList();

        public async Task<bool> TestAsync(string UID, CancellationToken cancel = default)
        {
            List<Task<bool>> tasks = _providers.ConvertAll(x => x.TestAsync(UID, cancel));
            while (tasks.Count != 0)
            {
                Task<bool> task = await Task.WhenAny(tasks);
                if (task.Result)
                    return true;

                tasks.Remove(task);
            }
            return false;
        }
    }
}
