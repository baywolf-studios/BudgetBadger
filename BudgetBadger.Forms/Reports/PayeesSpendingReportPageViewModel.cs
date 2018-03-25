using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using Microcharts;
using Prism.AppModel;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using SkiaSharp;

namespace BudgetBadger.Forms.Reports
{
    public class PayeesSpendingReportPageViewModel : BindableBase, IPageLifecycleAware
    {
        readonly INavigationService _navigationService;
        readonly IReportLogic _reportLogic;

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

        Chart _payeeChart;
        public Chart PayeeChart
        {
            get => _payeeChart;
            set => SetProperty(ref _payeeChart, value);
        }

        public PayeesSpendingReportPageViewModel(INavigationService navigationService, IReportLogic reportLogic)
        {
            _navigationService = navigationService;
            _reportLogic = reportLogic;

            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());

            BeginDate = DateTime.MinValue;
            EndDate = DateTime.MaxValue;
        }

        public async void OnAppearing()
        {
            await ExecuteRefreshCommand();
        }

        public void OnDisappearing()
        {
        }

        public async Task ExecuteRefreshCommand()
        {
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

                var payeeReportResult = await _reportLogic.GetPayeeSpendingTotalsReport(beginDate, endDate);
                if (payeeReportResult.Success)
                {
                    foreach (var datapoint in payeeReportResult.Data)
                    {
                        payeeEntries.Add(new Entry((float)datapoint.Value)
                        {
                            Label = datapoint.Key,
                            ValueLabel = datapoint.Value.ToString("C"),
                            Color = SKColor.Parse("#4CAF50")
                        });
                    }
                }

                PayeeChart = new DonutChart() { Entries = payeeEntries };
            }
            finally
            {
                IsBusy = false;
                BusyText = string.Empty;
            }
        }
    }
}
