using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Logic;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Forms.Events;
using BudgetBadger.Core.Models;
using Prism.Events;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Payees
{
    public class PayeesPageViewModel : BaseViewModel, INavigatedAware
    {
        readonly Lazy<IResourceContainer> _resourceContainer;
        readonly Lazy<IPayeeLogic> _payeeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly IEventAggregator _eventAggregator;

        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand RefreshPayeeCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand SaveSearchCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand AddTransactionCommand { get; set; }
        public Predicate<object> Filter { get => (payee) => _payeeLogic.Value.FilterPayee((Payee)payee, SearchText); }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        ObservableList<Payee> _payees;
        public ObservableList<Payee> Payees
        {
            get => _payees;
            set => SetProperty(ref _payees, value);
        }

        Payee _selectedPayee;
        public Payee SelectedPayee
        {
            get => _selectedPayee;
            set => SetProperty(ref _selectedPayee, value);
        }

        public bool HasSearchText { get => !string.IsNullOrWhiteSpace(SearchText); }

        string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { SetProperty(ref _searchText, value); RaisePropertyChanged(nameof(HasSearchText)); }
        }

        bool _noPayees;
        public bool NoPayees
        {
            get => _noPayees;
            set => SetProperty(ref _noPayees, value);
        }

        bool _fullRefresh = true;

        public PayeesPageViewModel(Lazy<IResourceContainer> resourceContainer,
                                   INavigationService navigationService,
                                   IPageDialogService dialogService,
                                   Lazy<IPayeeLogic> payeeLogic,
                                   IEventAggregator eventAggregator)
        {
            _resourceContainer = resourceContainer;
            _payeeLogic = payeeLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;

            Payees = new ObservableList<Payee>();
            SelectedPayee = null;

            SelectedCommand = new Command<Payee>(async p => await ExecuteSelectedCommand(p));
            RefreshCommand = new Command(async () => await FullRefresh());
            SaveCommand = new Command<Payee>(async p => await ExecuteSaveCommand(p));
            SaveSearchCommand = new Command(async () => await ExecuteSaveSearchCommand());
            AddCommand = new Command(async () => await ExecuteAddCommand());
            EditCommand = new Command<Payee>(async a => await ExecuteEditCommand(a));
            AddTransactionCommand = new Command(async () => await ExecuteAddTransactionCommand());
            RefreshPayeeCommand = new Command<Payee>(RefreshPayee);

            _eventAggregator.GetEvent<PayeeSavedEvent>().Subscribe(RefreshPayee);
            _eventAggregator.GetEvent<PayeeDeletedEvent>().Subscribe(RefreshPayee);
            _eventAggregator.GetEvent<PayeeHiddenEvent>().Subscribe(RefreshPayee);
            _eventAggregator.GetEvent<PayeeUnhiddenEvent>().Subscribe(RefreshPayee);
            _eventAggregator.GetEvent<TransactionSavedEvent>().Subscribe(async t => await RefreshPayeeFromTransaction(t));
            _eventAggregator.GetEvent<TransactionDeletedEvent>().Subscribe(async t => await RefreshPayeeFromTransaction(t));
        }

        public override async void OnActivated()
        {
            if (_fullRefresh)
            {
                await FullRefresh();
                _fullRefresh = false;
            }
        }

        public override void OnDeactivated()
        {
            SelectedPayee = null;
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
        }

        public async Task ExecuteSelectedCommand(Payee payee)
        {
            if (payee == null)
            {
                return;
            }

            if (payee.IsGenericHiddenPayee)
            {
                await _navigationService.NavigateAsync(PageName.HiddenPayeesPage);
            }
            else
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.Payee, payee }
                };

                await _navigationService.NavigateAsync(PageName.PayeeInfoPage, parameters);
            }
        }

        public async Task ExecuteSaveCommand(Payee payee)
        {
            var result = await _payeeLogic.Value.SavePayeeAsync(payee);

            if (result.Success)
            {
                _eventAggregator.GetEvent<PayeeSavedEvent>().Publish(result.Data);
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertSaveUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
            }
        }

        public async Task ExecuteSaveSearchCommand()
        {
            var newPayee = new Payee
            {
                Description = SearchText
            };

            var result = await _payeeLogic.Value.SavePayeeAsync(newPayee);

            if (result.Success)
            {
                _eventAggregator.GetEvent<PayeeSavedEvent>().Publish(result.Data);
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertSaveUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
            }
        }

        public async Task ExecuteAddCommand()
        {
            await _navigationService.NavigateAsync(PageName.PayeeEditPage);

            SelectedPayee = null;
        }

        public async Task ExecuteEditCommand(Payee payee)
        {
            if (!payee.IsGenericHiddenPayee)
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.Payee, payee }
                };
                await _navigationService.NavigateAsync(PageName.PayeeEditPage, parameters);
            }
        }

        public async Task ExecuteAddTransactionCommand()
        {
            await _navigationService.NavigateAsync(PageName.TransactionEditPage);
        }

        public async Task FullRefresh()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                var result = await _payeeLogic.Value.GetPayeesAsync();

                if (result.Success)
                {
                    Payees.ReplaceRange(result.Data);
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                }

                RefreshSummary();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void RefreshSummary()
        {
            NoPayees = (Payees?.Count ?? 0) == 0;
        }

        public void RefreshPayee(Payee payee)
        {
            var payees = Payees.Where(a => a.Id != payee.Id).ToList();

            if (payee != null && _payeeLogic.Value.FilterPayee(payee, FilterType.Standard))
            {
                payees.Add(payee);
            }

            Payees.ReplaceRange(payees);
            RefreshSummary();
        }

        public async Task RefreshPayeeFromTransaction(Transaction transaction)
        {
            if (transaction != null && transaction.Payee != null)
            {
                var updatedPayeeResult = await _payeeLogic.Value.GetPayeeAsync(transaction.Payee.Id);
                if (updatedPayeeResult.Success)
                {
                    RefreshPayee(updatedPayeeResult.Data);
                }
            }
        }
    }
}

