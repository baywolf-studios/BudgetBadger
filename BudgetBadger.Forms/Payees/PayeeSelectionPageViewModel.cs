using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Logic;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Forms.Events;
using BudgetBadger.Models;
using BudgetBadger.Models.Extensions;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;

namespace BudgetBadger.Forms.Payees
{
    public class PayeeSelectionPageViewModel : BindableBase, INavigationAware, IInitializeAsync
    {
        readonly IResourceContainer _resourceContainer;
        readonly IPayeeLogic _payeeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly IEventAggregator _eventAggregator;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SaveSearchCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public Predicate<object> Filter { get => (payee) => _payeeLogic.FilterPayee((Payee)payee, SearchText); }

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

        public PayeeSelectionPageViewModel(IResourceContainer resourceContainer,
            INavigationService navigationService,
            IPageDialogService dialogService,
            IPayeeLogic payeeLogic,
            IEventAggregator eventAggregator)
        {
            _resourceContainer = resourceContainer;
            _payeeLogic = payeeLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;

            Payees = new ObservableList<Payee>();
            SelectedPayee = null;

            SelectedCommand = new DelegateCommand<Payee>(async p => await ExecuteSelectedCommand(p));
            RefreshCommand = new DelegateCommand(async () => await FullRefresh());
            SaveSearchCommand = new DelegateCommand(async () => await ExecuteSaveSearchCommand());
            AddCommand = new DelegateCommand(async () => await ExecuteAddCommand());

            _eventAggregator.GetEvent<PayeeSavedEvent>().Subscribe(RefreshPayee);
            _eventAggregator.GetEvent<PayeeDeletedEvent>().Subscribe(RefreshPayee);
            _eventAggregator.GetEvent<PayeeHiddenEvent>().Subscribe(RefreshPayee);
            _eventAggregator.GetEvent<PayeeUnhiddenEvent>().Subscribe(RefreshPayee);
            _eventAggregator.GetEvent<TransactionSavedEvent>().Subscribe(async t => await RefreshPayeeFromTransaction(t));
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            SelectedPayee = null;
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            var payee = parameters.GetValue<Payee>(PageParameter.Payee);
            if (payee != null)
            {
                await _navigationService.GoBackAsync(parameters);
                return;
            }
        }

        public async Task InitializeAsync(INavigationParameters parameters)
        {
            await FullRefresh();
        }

        public async Task ExecuteAddCommand()
        {
            await _navigationService.NavigateAsync(PageName.PayeeEditPage);
        }

        public async Task ExecuteSelectedCommand(Payee payee)
        {
            if (payee == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { PageParameter.Payee, payee }
            };

            await _navigationService.GoBackAsync(parameters);
        }

        public async Task ExecuteSaveSearchCommand()
        {
            var newPayee = new Payee
            {
                Description = SearchText
            };

            var result = await _payeeLogic.SavePayeeAsync(newPayee);

            if (result.Success)
            {
                _eventAggregator.GetEvent<PayeeSavedEvent>().Publish(result.Data);
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

        public async Task FullRefresh()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                var result = await _payeeLogic.GetPayeesForSelectionAsync();

                if (result.Success)
                {
                    Payees.ReplaceRange(result.Data);
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertRefreshUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                }

                NoPayees = (Payees?.Count ?? 0) == 0;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void RefreshPayee(Payee payee)
        {
            var payees = Payees.Where(a => a.Id != payee.Id).ToList();

            if (payee != null && payee.IsActive)
            {
                payees.Add(payee);
            }

            Payees.ReplaceRange(payees);
        }

        public async Task RefreshPayeeFromTransaction(Transaction transaction)
        {
            if (transaction != null && transaction.Payee != null)
            {
                var updatedPayeeResult = await _payeeLogic.GetPayeeAsync(transaction.Payee.Id);
                if (updatedPayeeResult.Success)
                {
                    RefreshPayee(updatedPayeeResult.Data);
                }
            }
        }
    }
}

