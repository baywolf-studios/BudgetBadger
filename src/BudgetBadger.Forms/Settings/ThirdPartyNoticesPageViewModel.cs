using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Forms.Enums;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Settings
{
	public class ThirdPartyNoticesPageViewModel : BaseViewModel, INavigatedAware
	{
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly IResourceContainer _resourceContainer;

        public ICommand BackCommand { get => new Command(async () => await _navigationService.GoBackAsync()); }
        public ICommand LicenseCommand { get; set; }
        public ICommand EmailCommand { get; set; }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public List<KeyValuePair<string, string>> _thirdPartyNotices;
        public List<KeyValuePair<string, string>> ThirdPartyNotices
        {
            get => _thirdPartyNotices;
            set => SetProperty(ref _thirdPartyNotices, value);
        }

        KeyValuePair<string, string> _selectedThirdPartyNotice;
        public KeyValuePair<string, string> SelectedThirdPartyNotice
        {
            get => _selectedThirdPartyNotice;
            set => SetProperty(ref _selectedThirdPartyNotice, value);
        }

        public ThirdPartyNoticesPageViewModel(INavigationService navigationService,
                                              IPageDialogService dialogService,
                                              IResourceContainer resourceContainer)
		{
            _navigationService = navigationService;
            _dialogService = dialogService;
            _resourceContainer = resourceContainer;

            LicenseCommand = new Command(async () => await ExecuteLicenseCommand());
            EmailCommand = new Command(async () => await ExecuteEmailCommand());

            ThirdPartyNotices = GetThirdPartyLicenses();
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            SelectedThirdPartyNotice = new KeyValuePair<string, string>();
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
        }

        public async Task ExecuteEmailCommand()
        {
            try
            {
                var message = new EmailMessage
                {
                    Subject = "Third Party Notices",
                    To = new List<string>() { "Support@BudgetBadger.io" }
                };
                await Email.ComposeAsync(message);
            }
            catch (Exception ex)
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertGeneralError"),
                        ex.Message,
                        _resourceContainer.GetResourceString("AlertOk"));
                
            }
        }

        public async Task ExecuteLicenseCommand()
        {
            if (!string.IsNullOrEmpty(SelectedThirdPartyNotice.Key))
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.LicenseName, SelectedThirdPartyNotice.Key },
                    { PageParameter.LicenseText, SelectedThirdPartyNotice.Value }
                };

                await _navigationService.NavigateAsync(PageName.LicensePage, parameters);
            }
        }

        List<KeyValuePair<string, string>> GetThirdPartyLicenses()
        {
            var result = new List<KeyValuePair<string, string>>();

            var assembly = typeof(ThirdPartyNoticesPageViewModel).Assembly;
            var assemblyName = assembly.GetName().Name;
            Stream stream = assembly.GetManifestResourceStream($"{assemblyName}.THIRD-PARTY-NOTICES");
            string text = "";
            using (var reader = new StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }

            string[] split1 = { "========================================================" };
            string[] split2 = { "--------------------------------------------------------" };

            var splitText = text.Split(split1, StringSplitOptions.None);

            foreach (var split in splitText.Skip(1))
            {
                var license = split.Split(split2, StringSplitOptions.None);
                var licenseName = license[0].Replace("License notice for ", "").Trim();
                var liceneseText = license[1];
                result.Add(new KeyValuePair<string, string>(licenseName, liceneseText));
            }

            return result;
        }
    }
}

