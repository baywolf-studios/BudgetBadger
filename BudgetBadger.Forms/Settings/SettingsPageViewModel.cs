using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Authentication;
using BudgetBadger.Core.Settings;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Models;
using Dropbox.Api;
using Prism.Commands;
using Prism.Navigation;
using PropertyChanged;

namespace BudgetBadger.Forms.Settings
{
    [AddINotifyPropertyChangedInterface]
    public class SettingsPageViewModel : INavigationAware
    {
        readonly INavigationService NavigationService;
        readonly ISettings Settings;

        public ICommand SyncCommand { get; set; }

        public string SyncMode { get; set; }

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
            SyncMode = Settings.GetValueOrDefault(SettingsKeys.SyncMode);
        }
    }
}
