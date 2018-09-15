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
    public class PayeeSelectionPageViewModel : BindableBase, INavigationAware
    {
        readonly IPayeeLogic _payeeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SaveCommand { get; set; }
		public ICommand AddCommand { get; set; }
        public Predicate<object> Filter { get => (payee) => _payeeLogic.FilterPayee((Payee)payee, SearchText); }

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

        public PayeeSelectionPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IPayeeLogic payeeLogic)
        {
            _payeeLogic = payeeLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;

            Payees = new List<Payee>();
            SelectedPayee = null;

            SelectedCommand = new DelegateCommand<Payee>(async p => await ExecuteSelectedCommand(p));
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
			AddCommand = new DelegateCommand(async () => await ExecuteAddCommand());
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public async void OnNavigatedTo(NavigationParameters parameters)
        {
            var payee = parameters.GetValue<Payee>(PageParameter.Payee);
            if (payee != null)
            {
                await _navigationService.GoBackAsync(parameters);
            }
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            await ExecuteRefreshCommand();
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

        public async Task ExecuteRefreshCommand()
        {
            if (!IsBusy)
            {
                IsBusy = true;
            }

            try
            {
                var result = await _payeeLogic.GetPayeesForSelectionAsync();

                if (result.Success)
                {
                    Payees = result.Data;
                }
                else
                {
                    await Task.Yield();
                    await _dialogService.DisplayAlertAsync("Error", result.Message, "OK");
                }

                NoPayees = (Payees?.Count ?? 0) == 0;
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
    }
}

