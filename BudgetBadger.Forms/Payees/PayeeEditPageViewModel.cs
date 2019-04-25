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

namespace BudgetBadger.Forms.Payees
{
    public class PayeeEditPageViewModel : BindableBase, INavigationAware
    {
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
        public ICommand DeleteCommand { get; set; }
        public ICommand UndoDeleteCommand { get; set; }

        public PayeeEditPageViewModel(INavigationService navigationService,
                                      IPageDialogService dialogService,
                                      IPayeeLogic payeeLogic,
                                      ISyncFactory syncFactory)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            _payeeLogic = payeeLogic;
            _syncFactory = syncFactory;

            Payee = new Payee();

            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
            DeleteCommand = new DelegateCommand(async () => await ExecuteDeleteCommand());
            UndoDeleteCommand = new DelegateCommand(async () => await ExecuteUndoDeleteCommand());
        }

        public void OnNavigatingTo(INavigationParameters parameters)
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
                BusyText = "Saving";
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
                    await _dialogService.DisplayAlertAsync("Save Unsuccessful", result.Message, "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteDeleteCommand()
        {
			if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                BusyText = "Deleting";
                var result = await _payeeLogic.DeletePayeeAsync(Payee.Id);
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
                    await _dialogService.DisplayAlertAsync("Delete Unsuccessful", result.Message, "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteUndoDeleteCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                BusyText = "Undoing Delete";
                var result = await _payeeLogic.UndoDeletePayeeAsync(Payee.Id);
                if (result.Success)
                {
                    _needToSync = true;

                    await _navigationService.GoBackAsync();
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("Delete Unsuccessful", result.Message, "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
