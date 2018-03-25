using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;

namespace BudgetBadger.Forms.Payees
{
    public class PayeeSelectionPageViewModel : BindableBase, INavigatingAware
    {
        readonly IPayeeLogic _payeeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;

        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        IReadOnlyList<Payee> _payees;
        public IReadOnlyList<Payee> Payees
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

        IReadOnlyList<IGrouping<string, Payee>> _groupedPayees;
        public IReadOnlyList<IGrouping<string, Payee>> GroupedPayees
        {
            get => _groupedPayees;
            set => SetProperty(ref _groupedPayees, value);
        }

        public bool HasSearchText { get => !string.IsNullOrWhiteSpace(SearchText); }

        string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { SetProperty(ref _searchText, value); RaisePropertyChanged("HasSearchText"); }
        }

        public PayeeSelectionPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IPayeeLogic payeeLogic)
        {
            _payeeLogic = payeeLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;

            Payees = new List<Payee>();
            SelectedPayee = null;
            GroupedPayees = Payees.GroupBy(p => "").ToList();

            SelectedCommand = new DelegateCommand(async () => await ExecuteSelectedCommand());
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
            SearchCommand = new DelegateCommand(ExecuteSearchCommand);
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            await ExecuteRefreshCommand();
        }

        public async Task ExecuteSelectedCommand()
        {
            if (SelectedPayee == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { PageParameter.Payee, SelectedPayee }
            };

            await _navigationService.GoBackAsync(parameters);

            SelectedPayee = null;
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
                var result = await _payeeLogic.GetPayeesForSelectionAsync();

                if (result.Success)
                {
                    Payees = result.Data;
                    GroupedPayees = _payeeLogic.GroupPayees(Payees);
                }
                else
                {
                    await Task.Yield();
                    await _dialogService.DisplayAlertAsync("Error", result.Message, "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteSaveCommand()
        {
            var newPayee = new Payee
            {
                Description = SearchText
            };

            var result = await _payeeLogic.SavePayeeAsync(newPayee);

            if (result.Success)
            {
                var parameters = new NavigationParameters
                {
                    { PageParameter.Payee, result.Data }
                };

                await _navigationService.GoBackAsync(parameters);
            }
            else
            {
                //show error
            }
        }

        public void ExecuteSearchCommand()
        {
            GroupedPayees = _payeeLogic.GroupPayees(_payeeLogic.SearchPayees(Payees, SearchText));
        }
    }
}

