using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fuck.Models;
using Fuck.Services.Logging;
using Fuck.Views;
using RucSu.Services;

namespace Fuck.ViewModels
{
    public partial class LogsViewModel(IServiceProvider services, DBContext db) : ObservableObject
    {
        public DateTime First { get; set; }
        public DateTime Second { get; set; }

        [ObservableProperty]
        private List<FuckLog>? _logs;

        [RelayCommand]
        private void Load()
        {
            if (db is not DBContextWithLogging logsKeeper)
                return;

            Logs = First > Second
                ? logsKeeper.GetLogs(Second, First)
                : logsKeeper.GetLogs(First, Second);
        }

        [RelayCommand]
        public void ToHubPage() => services.GetRequiredService<App>().SetPage<HubPage>();
    }
}
