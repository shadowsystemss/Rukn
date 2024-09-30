namespace AuthLib
{
    public class NetworkAuthProvider(HttpClient httpClient, string url) : IAuthProvider
    {
        public async Task<bool> TestAsync(string UID, CancellationToken cancel = default)
        {
            try
            {
                string data = await httpClient.GetStringAsync(url, cancel);
                return data.Split('\n').Contains(UID);
            }
            catch
            {
                return false;
            }
        }
    }
}
