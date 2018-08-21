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
using SkiaSharp;

namespace BudgetBadger.Forms.Reports
{
    public class PayeeTrendReportPageViewModel: BindableBase, IPageLifecycleAware
    {
        readonly INavigationService _navigationService;
        readonly IReportLogic _reportLogic;
        readonly IPayeeLogic _payeeLogic;

        public ICommand RefreshCommand { get; set; }

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

        bool _dateRangeFilter;
        public bool DateRangeFilter
        {
            get => _dateRangeFilter;
            set => SetProperty(ref _dateRangeFilter, value);
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

        Payee _selectedPayee;
        public Payee SelectedPayee
        {
            get => _selectedPayee;
            set => SetProperty(ref _selectedPayee, value);
        }

        IReadOnlyList<Payee> _payees;
        public IReadOnlyList<Payee> Payees
        {
            get => _payees;
            set => SetProperty(ref _payees, value);
        }

        Chart _payeeChart;
        public Chart PayeeChart
        {
            get => _payeeChart;
            set => SetProperty(ref _payeeChart, value);
        }

        public PayeeTrendReportPageViewModel(INavigationService navigationService, IPayeeLogic payeeLogic, IReportLogic reportLogic)
        {
            _navigationService = navigationService;
            _reportLogic = reportLogic;
            _payeeLogic = payeeLogic;

            Payees = new List<Payee>();

            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());

            BeginDate = DateTime.MinValue;
            EndDate = DateTime.MaxValue;
        }

        public async void OnAppearing()
        {
            var payeesResult = await _payeeLogic.GetPayeesForSelectionAsync();
            if (payeesResult.Success)
            {
                Payees = payeesResult.Data.ToList();
                SelectedPayee = Payees.FirstOrDefault();
            }
            else
            {
                //show some error
            }

            await ExecuteRefreshCommand();
        }

        public void OnDisappearing()
        {
        }

        public async Task ExecuteRefreshCommand()
        {
            if (SelectedPayee == null)
            {
                return;
            }

            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            BusyText = "Loading...";

            try
            {
                var payeeEntries = new List<Entry>();

                var beginDate = DateRangeFilter ? (DateTime?)BeginDate : null;
                var endDate = DateRangeFilter ? (DateTime?)EndDate : null;

                //var payeeReportResult = await _reportLogic.GetSpendingTrendsByPayeeReport(SelectedPayee.Id, beginDate, endDate);
                //if (payeeReportResult.Success)
                //{
                //    foreach (var datapoint in payeeReportResult.Data)
                //    {
                //        var color = SKColor.Parse("#4CAF50");
                //        if (datapoint.Value < 0)
                //        {
                //            color = SKColor.Parse("#F44336");
                //        }

                //        payeeEntries.Add(new Entry((float)datapoint.Value)
                //        {
                //            Label = datapoint.Key.ToString("Y"),
                //            ValueLabel = datapoint.Value.ToString("C"),
                //            Color = color
                //        });
                //    }
                //}

                PayeeChart = new PointChart() { Entries = payeeEntries };
            }
            finally
            {
                IsBusy = false;
                BusyText = string.Empty;
            }
        }
    }
}

