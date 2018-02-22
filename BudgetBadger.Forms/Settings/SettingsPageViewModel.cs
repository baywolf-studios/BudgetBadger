using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Settings;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Models;
using Dropbox.Api;
using Prism.AppModel;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;

namespace BudgetBadger.Forms.Settings
{
    public class SettingsPageViewModel : BindableBase, INavigationAware, IPageLifecycleAware
    {
        readonly INavigationService NavigationService;
        readonly ISettings Settings;

        public ICommand SyncCommand { get; set; }

        string _syncMode;
        public string SyncMode
        {
            get { return _syncMode; }
            set { SetProperty(ref _syncMode, value); }
        }

        public SettingsPageViewModel(INavigationService navigationService,
                                    ISettings settings)
        {
            NavigationService = navigationService;
            Settings = settings;

            SyncCommand = new DelegateCommand(async () => await ExecuteSyncCommand());
        }

        public async Task ExecuteSyncCommand()
        {
            await NavigationService.NavigateAsync(PageName.SyncPage);
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
            SyncMode = Settings.GetValueOrDefault(AppSettings.SyncMode);
        }

        public void OnAppearing()
        {
            SyncMode = Settings.GetValueOrDefault(AppSettings.SyncMode);
        }

        public void OnDisappearing()
        {
        }
    }
}
