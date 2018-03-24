using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Forms.Enums;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Collections.Generic;
using System.Linq;
using Prism.Mvvm;
using Prism.AppModel;

namespace BudgetBadger.Forms.Payees
{
    public class PayeesPageViewModel : BindableBase, INavigatingAware, IPageLifecycleAware
    {
        readonly IPayeeLogic _payeeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;

        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

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
        public IReadOnlyList<IGrouping<string, Payee>>  GroupedPayees
        {
            get => _groupedPayees;
            set => SetProperty(ref _groupedPayees, value);
        }

        public bool NoSearchResults { get => !string.IsNullOrWhiteSpace(SearchText) && GroupedPayees.Count == 0; }

        string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public PayeesPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IPayeeLogic payeeLogic)
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
            AddCommand = new DelegateCommand(async () => await ExecuteAddCommand());
            EditCommand = new DelegateCommand<Payee>(async a => await ExecuteEditCommand(a));
            DeleteCommand = new DelegateCommand<Payee>(async a => await ExecuteDeleteCommand(a));
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            await ExecuteRefreshCommand();
        }

        public async void OnAppearing()
        {
            await ExecuteRefreshCommand();
        }

        public void OnDisappearing()
        {
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

            await _navigationService.NavigateAsync(PageName.PayeeInfoPage, parameters);

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
                var result = await _payeeLogic.GetPayeesAsync();

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

        public async Task ExecuteAddCommand()
        {
            await _navigationService.NavigateAsync(PageName.PayeeEditPage);

            SelectedPayee = null;
        }

        public async Task ExecuteEditCommand(Payee payee)
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.Payee, payee }
            };
            await _navigationService.NavigateAsync(PageName.PayeeEditPage, parameters);
        }

        public async Task ExecuteDeleteCommand(Payee payee)
        {
            var result = await _payeeLogic.DeletePayeeAsync(payee.Id);

            if (result.Success)
            {
                await ExecuteRefreshCommand();
            }
            else
            {
                await _dialogService.DisplayAlertAsync("Delete Unsuccessful", result.Message, "OK");
            }
        }

        public void ExecuteSearchCommand()
        {
            GroupedPayees = _payeeLogic.GroupPayees(_payeeLogic.SearchPayees(Payees, SearchText));
            RaisePropertyChanged("NoSearchResults");
        }
    }
}

