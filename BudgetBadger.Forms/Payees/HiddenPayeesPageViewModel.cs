using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Logic;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Models;
using BudgetBadger.Models.Extensions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;

namespace BudgetBadger.Forms.Payees
{
    public class HiddenPayeesPageViewModel : BindableBase, INavigatedAware, IInitializeAsync
    {
        readonly IResourceContainer _resourceContainer;
        readonly IPayeeLogic _payeeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand RefreshPayeeCommand { get; set; }
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
            set { SetProperty(ref _searchText, value); RaisePropertyChanged("HasSearchText"); }
        }

        bool _noPayees;
        public bool NoPayees
        {
            get => _noPayees;
            set => SetProperty(ref _noPayees, value);
        }

        public HiddenPayeesPageViewModel(
            IResourceContainer resourceContainer,
            INavigationService navigationService,
            IPageDialogService dialogService,
            IPayeeLogic payeeLogic)
        {
            _resourceContainer = resourceContainer;
            _payeeLogic = payeeLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;

            Payees = new ObservableList<Payee>();
            SelectedPayee = null;

            SelectedCommand = new DelegateCommand<Payee>(async p => await ExecuteSelectedCommand(p));
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            RefreshPayeeCommand = new DelegateCommand<Payee>(async a => await ExecuteRefreshPayeeCommand(a));
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            SelectedPayee = null;
        }

        public async Task InitializeAsync(INavigationParameters parameters)
        {
            await ExecuteRefreshCommand();
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.TryGetValue(PageParameter.Payee, out Payee payee))
            {
                await ExecuteRefreshPayeeCommand(payee);
            }

            if (parameters.TryGetValue(PageParameter.Transaction, out Transaction transaction))
            {
                await ExecuteRefreshPayeeCommand(transaction.Payee);
            }
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

            await _navigationService.NavigateAsync(PageName.PayeeEditPage, parameters);
        }

        public async Task ExecuteRefreshCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                var result = await _payeeLogic.GetPayeesAsync();

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

        public async Task ExecuteRefreshPayeeCommand(Payee payee)
        {
            var updatedPayee = await _payeeLogic.GetPayeeAsync(payee.Id);
            if (updatedPayee.Success)
            {
                var payeeToRemove = Payees.FirstOrDefault(a => a.Id == payee.Id);
                Payees.Remove(payeeToRemove);
                Payees.Add(updatedPayee.Data);
            }
        }
    }
}

