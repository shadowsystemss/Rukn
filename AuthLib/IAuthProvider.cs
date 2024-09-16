namespace AuthLib
{
    public interface IAuthProvider
    {
        public Task<bool> TestAsync(string UID, CancellationToken cancel = default);
    }
}
