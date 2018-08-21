using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using Microcharts;
using Prism.AppModel;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using SkiaSharp;

namespace BudgetBadger.Forms.Reports
{
    public class PayeesSpendingReportPageViewModel : BindableBase, INavigatingAware
    {
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly IReportLogic _reportLogic;

        public ICommand RefreshCommand { get; set; }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        DateTime _beginDate;
        public DateTime BeginDate
        {
            get => _beginDate;
            set => SetProperty(ref _beginDate, value);
        }

        DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        IReadOnlyList<DataPoint<Payee, decimal>> _payees;
        public IReadOnlyList<DataPoint<Payee, decimal>> Payees
        {
            get => _payees;
            set => SetProperty(ref _payees, value);
        }

        DataPoint<Payee, decimal> _selectedPayee;
        public DataPoint<Payee, decimal> SelectedPayee
        {
            get => _selectedPayee;
            set => SetProperty(ref _selectedPayee, value);
        }

        public PayeesSpendingReportPageViewModel(INavigationService navigationService,
                                                 IPageDialogService dialogService,
                                                 IReportLogic reportLogic)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            _reportLogic = reportLogic;

            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());

            var now = DateTime.Now;
            EndDate = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddTicks(-1);
            BeginDate = EndDate.AddMonths(-12);
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            await ExecuteRefreshCommand();
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
                var payeeEntries = new List<Entry>();

                var beginDate = (DateTime?)BeginDate;
                var endDate = (DateTime?)EndDate;

                var payeeReportResult = await _reportLogic.GetPayeesSpendingReport(beginDate, endDate);
                if (payeeReportResult.Success)
                {
                    Payees = payeeReportResult.Data;
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("Error", payeeReportResult.Message, "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
