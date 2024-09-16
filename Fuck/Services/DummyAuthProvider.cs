using AuthLib;

namespace Fuck.Services
{
    public sealed class DummyAuthProvider : IAuthProvider
    {
        public Task<bool> TestAsync(string UID, CancellationToken cancel = default) => Task.FromResult(true);
    }
}
