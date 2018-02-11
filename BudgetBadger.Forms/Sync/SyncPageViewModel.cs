using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Settings;
using BudgetBadger.Core.Sync;
using BudgetBadger.Forms.Enums;
using Prism.Commands;
using Prism.Navigation;

namespace BudgetBadger.Forms.Sync
{
    public class SyncPageViewModel : INavigationAware
    {
        readonly INavigationService NavigationService;
        readonly ISync SyncService;
        readonly ISettings Settings;

        public ICommand SyncCommand { get; set; }
        public ICommand SyncModeSelectedCommand { get; set; }

        public string SyncMode { get; set; }
        public bool SyncModeSelected { get => !string.IsNullOrEmpty(SyncMode); }

        public SyncPageViewModel(INavigationService navigationService,
                                 ISettings settings,
                                 ISync syncService)
        {
            NavigationService = navigationService;
            SyncService = syncService;
            Settings = settings;

            SyncCommand = new DelegateCommand(async () => await ExecuteSyncCommand());
            SyncModeSelectedCommand = new DelegateCommand(async () => await ExecuteSyncModeSelectedCommand());
        }

        public async Task ExecuteSyncCommand()
        {
            await SyncService.FullSync();
        }

        public async Task ExecuteSyncModeSelectedCommand()
        {
            await NavigationService.NavigateAsync(PageName.FileSyncProvidersPage);
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
            SyncMode = Settings.GetValueOrDefault(SettingsKeys.FileSyncProvider);
        }
    }
}
