using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Forms.Enums;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using Prism.Mvvm;
using BudgetBadger.Core.Sync;
using Xamarin.Forms;
using BudgetBadger.Core.LocalizedResources;

namespace BudgetBadger.Forms.Payees
{
    public class PayeeEditPageViewModel : BindableBase, INavigationAware, IInitializeAsync
    {
        readonly IResourceContainer _resourceContainer;
        readonly IPayeeLogic _payeeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ISyncFactory _syncFactory;

        bool _needToSync;

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        string _busyText;
        public string BusyText
        {
            get => _busyText;
            set => SetProperty(ref _busyText, value);
        }

        Payee _payee;
        public Payee Payee
        {
            get => _payee;
            set => SetProperty(ref _payee, value);
        }

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand SaveCommand { get; set; }
        public ICommand SoftDeleteCommand { get; set; }
        public ICommand UnhideCommand { get; set; }
        public ICommand HideCommand { get; set; }

        public PayeeEditPageViewModel(IResourceContainer resourceContainer,
            INavigationService navigationService,
                                      IPageDialogService dialogService,
                                      IPayeeLogic payeeLogic,
                                      ISyncFactory syncFactory)
        {
            _resourceContainer = resourceContainer;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _payeeLogic = payeeLogic;
            _syncFactory = syncFactory;

            Payee = new Payee();

            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
            SoftDeleteCommand = new DelegateCommand(async () => await ExecuteSoftDeleteCommand());
            HideCommand = new DelegateCommand(async () => await ExecuteHideCommand());
            UnhideCommand = new DelegateCommand(async () => await ExecuteUnhideCommand());
        }

        public async Task InitializeAsync(INavigationParameters parameters)
        {
            var payee = parameters.GetValue<Payee>(PageParameter.Payee);
            if (payee != null)
            {
                Payee = payee.DeepCopy();
            }
        }

        public async void OnNavigatedFrom(INavigationParameters parameters)
        {
            if (_needToSync)
            {
                var syncService = _syncFactory.GetSyncService();
                var syncResult = await syncService.FullSync();

                if (syncResult.Success)
                {
                    await _syncFactory.SetLastSyncDateTime(DateTime.Now);
                    _needToSync = false;
                }
            }
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
        }

        public async Task ExecuteSaveCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                BusyText = _resourceContainer.GetResourceString("BusyTextSaving");
                var result = await _payeeLogic.SavePayeeAsync(Payee);

                if (result.Success)
                {
                    _needToSync = true;

					var parameters = new NavigationParameters
                    {
                        { PageParameter.Payee, result.Data }
                    };               
                    await _navigationService.GoBackAsync(parameters);
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertSaveUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteSoftDeleteCommand()
        {
			if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                BusyText = _resourceContainer.GetResourceString("BusyTextDeleting");
                var result = await _payeeLogic.SoftDeletePayeeAsync(Payee.Id);
                if (result.Success)
                {
                    _needToSync = true;

                    if (Device.RuntimePlatform == Device.macOS)
                    {
                        await _navigationService.GoBackAsync();
                    }
                    else
                    {
                        await _navigationService.GoBackToRootAsync();
                    }
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertDeleteUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteHideCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                BusyText = _resourceContainer.GetResourceString("BusyTextHiding");
                var result = await _payeeLogic.HidePayeeAsync(Payee.Id);
                if (result.Success)
                {
                    _needToSync = true;

                    if (Device.RuntimePlatform == Device.macOS)
                    {
                        await _navigationService.GoBackAsync();
                    }
                    else
                    {
                        await _navigationService.GoBackToRootAsync();
                    }
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertHideUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteUnhideCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                BusyText = _resourceContainer.GetResourceString("BusyTextUnhiding");
                var result = await _payeeLogic.UnhidePayeeAsync(Payee.Id);
                if (result.Success)
                {
                    _needToSync = true;

                    await _navigationService.GoBackAsync();
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertUnhideUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
