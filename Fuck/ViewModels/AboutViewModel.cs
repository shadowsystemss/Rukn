using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fuck.Resources.Strings;
using Fuck.Views;
using System.ComponentModel;

namespace Fuck.ViewModels
{
    public partial class AboutViewModel : ObservableObject
    {
        private readonly static Dictionary<string, string> _documents = new()
        {
            {"Пользовательское соглашение", "license.txt"},
            {"Старт", "start.txt"},
        };

        public Page? BackPage { get; set; }
        private readonly IServiceProvider _services;

        [ObservableProperty]
        private string _text = AppResources.Loading;

        [ObservableProperty]
        private bool _dontShowAnymore;

        [ObservableProperty]
        private string _document;

        public List<string> Docs => [.. _documents.Keys];

        public AboutViewModel(IServiceProvider services)
        {
            _services = services;
            _dontShowAnymore = Preferences.Get("version", null) == AppInfo.Version.ToString();
            Document = "Пользовательское соглашение";
        }

        [RelayCommand]
        private async Task Load()
        {
            if (!_documents.TryGetValue(Document, out string? value))
                return;

            using Stream stream = await FileSystem.OpenAppPackageFileAsync(value);
            using var reader = new StreamReader(stream);
            Text = reader.ReadToEnd();
        }

        [RelayCommand]
        public void Close()
        {
            if (BackPage is not null && Document == "Пользовательское соглашение")
            {
                Document = "Старт";
                return;
            }

            _services.GetRequiredService<App>().SetPage(BackPage ?? _services.GetRequiredService<HubPage>());
            if (DontShowAnymore)
                Preferences.Set("version", AppInfo.Version.ToString());
            BackPage = null;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(Document))
                _ = Load();
        }
    }
}
