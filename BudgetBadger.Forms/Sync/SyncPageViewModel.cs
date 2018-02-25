using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Settings;
using BudgetBadger.Core.Sync;
using BudgetBadger.Forms.Enums;
using Prism.AppModel;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;

namespace BudgetBadger.Forms.Sync
{
    public class SyncPageViewModel : BindableBase, IPageLifecycleAware
    {
        readonly INavigationService NavigationService;
        readonly ISync SyncService;
        readonly ISettings Settings;

        public ICommand SyncCommand { get; set; }
        public ICommand SyncModeSelectedCommand { get; set; }

        string _syncMode;
        public string SyncMode
        {
            get => _syncMode;
            set => SetProperty(ref _syncMode, value);
        }
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

        public async void OnAppearing()
        {
            SyncMode = Settings.GetValueOrDefault(AppSettings.SyncMode);
        }

        public void OnDisappearing()
        {
        }

        public async Task ExecuteSyncCommand()
        {
            await SyncService.FullSync();
        }

        public async Task ExecuteSyncModeSelectedCommand()
        {
            await NavigationService.NavigateAsync(PageName.SyncModesPage);
        }
    }
}
