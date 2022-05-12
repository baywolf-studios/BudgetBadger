using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Logic;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Forms.Events;
using BudgetBadger.Models;
using Prism.Events;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Payees
{
    public class PayeeEditPageViewModel : ObservableBase, INavigationAware, IInitialize
    {
        readonly IResourceContainer _resourceContainer;
        readonly IPayeeLogic _payeeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly IEventAggregator _eventAggregator;

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

        public ICommand BackCommand { get => new Command(async () => await _navigationService.GoBackAsync()); }
        public ICommand SaveCommand { get; set; }
        public ICommand SoftDeleteCommand { get; set; }
        public ICommand UnhideCommand { get; set; }
        public ICommand HideCommand { get; set; }

        public PayeeEditPageViewModel(IResourceContainer resourceContainer,
            INavigationService navigationService,
                                      IPageDialogService dialogService,
                                      IPayeeLogic payeeLogic,
                                      IEventAggregator eventAggregator)
        {
            _resourceContainer = resourceContainer;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _payeeLogic = payeeLogic;
            _eventAggregator = eventAggregator;

            Payee = new Payee();

            SaveCommand = new Command(async () => await ExecuteSaveCommand());
            SoftDeleteCommand = new Command(async () => await ExecuteSoftDeleteCommand());
            HideCommand = new Command(async () => await ExecuteHideCommand());
            UnhideCommand = new Command(async () => await ExecuteUnhideCommand());
        }

        public void Initialize(INavigationParameters parameters)
        {
            var payee = parameters.GetValue<Payee>(PageParameter.Payee);
            if (payee != null)
            {
                Payee = payee.DeepCopy();
            }
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.GetNavigationMode() == NavigationMode.Back)
            {
                Initialize(parameters);
            }
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
                    _eventAggregator.GetEvent<PayeeSavedEvent>().Publish(result.Data);
          
                    await _navigationService.GoBackAsync();
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

            var confirm = await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertConfirmation"),
                _resourceContainer.GetResourceString("AlertConfirmDelete"),
                _resourceContainer.GetResourceString("AlertOk"),
                _resourceContainer.GetResourceString("AlertCancel"));

            if (confirm)
            {
                IsBusy = true;

                try
                {
                    BusyText = _resourceContainer.GetResourceString("BusyTextDeleting");
                    var result = await _payeeLogic.SoftDeletePayeeAsync(Payee.Id);
                    if (result.Success)
                    {
                        _eventAggregator.GetEvent<PayeeDeletedEvent>().Publish(result.Data);

                        await _navigationService.GoBackAsync();
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
                    _eventAggregator.GetEvent<PayeeHiddenEvent>().Publish(result.Data);

                    await _navigationService.GoBackAsync();
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
                    _eventAggregator.GetEvent<PayeeUnhiddenEvent>().Publish(result.Data);

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
