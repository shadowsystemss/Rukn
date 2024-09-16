using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fuck.Services;
using Fuck.Views;

namespace Fuck.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IServiceProvider _services;

        public SettingsViewModel(IServiceProvider services, AuthService auth, ProfileView profile)
        {
            _services = services;
            Profile = profile;

            UID = auth.GetUID();
        }

        public ProfileView Profile { get; init; }

        [ObservableProperty]
        private string _UID;

        [RelayCommand]
        public void ToBack() => _services.GetRequiredService<App>().SetPage<HubPage>();

        [RelayCommand]
        public static async Task TelegramAsync() => await AuthService.TelegramAsync();

        [RelayCommand]
        public async Task CopyUIDAsync() => await Clipboard.SetTextAsync(_services.GetRequiredService<AuthService>().GetUID());
    }
}
