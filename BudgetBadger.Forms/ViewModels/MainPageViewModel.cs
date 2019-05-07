using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Logic;
using BudgetBadger.Core.Settings;
using BudgetBadger.Forms.Enums;
using Prism.Commands;
using Prism.Ioc;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

namespace BudgetBadger.Forms.ViewModels
{
    public class MainPageViewModel : INavigatingAware
    {
        readonly INavigationService _navigationService;
        readonly ITransactionLogic _transactionLogic;
        readonly ISettings _settings;
        readonly IPageDialogService _dialogService;
        readonly IResourceContainer _resourceContainer;

        public ICommand NavigateCommand { get; set; }

        public MainPageViewModel(INavigationService navigationService,
            ITransactionLogic transactionLogic,
            ISettings settings,
            IPageDialogService dialogService,
            IResourceContainer resourceContainer)
        {
            _navigationService = navigationService;
            _transactionLogic = transactionLogic;
            _settings = settings;
            _dialogService = dialogService;
            _resourceContainer = resourceContainer;

            NavigateCommand = new DelegateCommand<string>(async a => await ExecuteNavigateCommand(a));
        }

        public async Task ExecuteNavigateCommand(string pageName)
        {
            if (!string.IsNullOrEmpty(pageName))
            {
                await _navigationService.NavigateAsync(pageName);
            }
        }

        public async void OnNavigatingTo(INavigationParameters parameters)
        {
            bool.TryParse(_settings.GetValueOrDefault(AppSettings.AskedForReview), out bool alreadyAskedForRevew);

            if (!alreadyAskedForRevew)
            {
                int.TryParse(_settings.GetValueOrDefault(AppSettings.AppOpenedCount), out int appOpenedCount);

                if (appOpenedCount >= 10)
                {
                    var transactionCountResult = await _transactionLogic.GetTransactionsCountAsync();
                    if (transactionCountResult.Success && transactionCountResult.Data >= 20)
                    {
                        await _settings.AddOrUpdateValueAsync(AppSettings.AskedForReview, true.ToString());

                        var enjoyingBudgetBadger = await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertBudgetBadger"),
                            _resourceContainer.GetResourceString("AlertMessageEnjoyingBudgetBadger"),
                            _resourceContainer.GetResourceString("AlertYes"),
                            _resourceContainer.GetResourceString("AlertNotReally"));

                        if (enjoyingBudgetBadger)
                        {
                            var wantToReview = await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertReview"),
                            _resourceContainer.GetResourceString("AlertMessageReview"),
                            _resourceContainer.GetResourceString("AlertOkSure"),
                            _resourceContainer.GetResourceString("AlertNoThanks"));

                            if (wantToReview)
                            {
                                switch (Device.RuntimePlatform)
                                {
                                    case Device.iOS:
                                        Device.OpenUri(new Uri("https://itunes.apple.com/us/app/budget-badger/id1436425263?mt=8&action=write-review"));
                                        break;
                                    case Device.Android:
                                        Device.OpenUri(new Uri("https://play.google.com/store/apps/details?id=com.BayWolfStudios.BudgetBadger"));
                                        break;
                                    case Device.UWP:
                                        Device.OpenUri(new Uri("ms-windows-store://review/?ProductId=9nps726fkxtt"));
                                        break;
                                    case Device.macOS:
                                        Device.OpenUri(new Uri("macappstore://itunes.apple.com/app/id1462666544?mt=12"));
                                        break;
                                }
                            }
                        }
                        else
                        {
                            var giveFeedback = await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertFeedback"),
                            _resourceContainer.GetResourceString("AlertMessageFeedback"),
                            _resourceContainer.GetResourceString("AlertOkSure"),
                            _resourceContainer.GetResourceString("AlertNoThanks"));

                            if (giveFeedback)
                            {
                                var emailUri = new Uri("mailto:support@BudgetBadger.io?subject=Feedback");
                                Device.OpenUri(emailUri);
                            }
                        }
                    }
                }
            }
        }
    }
}
