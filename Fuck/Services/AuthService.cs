using AuthLib;

namespace Fuck.Services;

public class AuthService
{
    private string? UID;
    private DateTime last;

    private const int HoursAge = 48;
    private const int Delay = 60000;

    private readonly string lastPath = Path.Combine(FileSystem.Current.CacheDirectory, "last");

    private readonly IAuthProvider _authProvider;

    public AuthService(IAuthProvider authProvider)
    {
        _authProvider = authProvider;

        if (File.Exists(lastPath))
            DateTime.TryParse(File.ReadAllText(lastPath), out last);

        if (Preferences.ContainsKey("UID"))
            UID = Preferences.Get("UID", null);
    }

    public string GetUID()
    {
        if (UID is not null)
            return UID;

        _ = TelegramAsync();
        UID = Guid.NewGuid().ToString();
        Preferences.Set("UID", UID);
        return UID;
    }

    public async Task<bool> TestAsync()
    {
        if ((DateTime.Now - last).TotalHours < HoursAge)
            return true;

        if (await _authProvider.TestAsync(GetUID()))
        {
            last = DateTime.Now;
            File.WriteAllText(lastPath, last.ToString());
            return true;
        }

        return false;
    }

    public static async Task TelegramAsync()
    {
        if (!await Launcher.TryOpenAsync("tg://join?invite=u69LXg_vlgYyNzMy"))
            await Launcher.OpenAsync(@"https://t.me/+u69LXg_vlgYyNzMy");
    }

    public static IAuthProvider Build(IServiceProvider services)
    {
        HttpClient client = services.GetRequiredService<HttpClient>();

        return new TimeoutAuthProvider(
            new MultiAuthProvider(
#if DEBUG
            new DummyAuthProvider(),
#endif
            new NetworkAuthProvider(client, @"https://raw.githubusercontent.com/shadowsystemss/Update/main/RusuUID"),
            new NetworkAuthProvider(client, @"https://pastebin.com/raw/sM3gynLW")),
            20000);
    }
}
