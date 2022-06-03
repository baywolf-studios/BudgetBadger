using System;
using System.Windows.Input;
using BudgetBadger.Forms.Enums;
using Prism.Navigation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Settings
{
	public class LicensePageViewModel : BaseViewModel, IInitialize
	{
        readonly INavigationService _navigationService;

        public ICommand BackCommand => new Command(async () => await _navigationService.GoBackAsync()); 

        string _licenseName;
        public string LicenseName
        {
            get => _licenseName;
            set => SetProperty(ref _licenseName, value);
        }

        string _licenseText;
        public string LicenseText
        {
            get => _licenseText;
            set => SetProperty(ref _licenseText, value);
        }

        public LicensePageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public void Initialize(INavigationParameters parameters)
        {
            LicenseName = parameters.GetValue<string>(PageParameter.LicenseName);
            LicenseText = parameters.GetValue<string>(PageParameter.LicenseText);
        }
    }
}

