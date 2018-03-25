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
    public class NetWorthReportPageViewModel : BindableBase, IPageLifecycleAware
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

        Chart _netWorthChart;
        public Chart NetWorthChart
        {
            get => _netWorthChart;
            set => SetProperty(ref _netWorthChart, value);
        }

        public NetWorthReportPageViewModel(INavigationService navigationService, IReportLogic reportLogic)
        {
            _navigationService = navigationService;
            _reportLogic = reportLogic;

            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());

            BeginDate = DateTime.MinValue;
            EndDate = DateTime.Now;
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
                var entries = new List<Entry>();

                var beginDate = DateRangeFilter ? (DateTime?)BeginDate : null;
                var endDate = DateRangeFilter ? (DateTime?)EndDate : null;

                var netWorthReportResult = await _reportLogic.GetNetWorthReport(beginDate, endDate);
                if (netWorthReportResult.Success)
                {
                    foreach (var dataPoint in netWorthReportResult.Data.OrderBy(d => d.Key))
                    {
                        var color = SKColor.Parse("#4CAF50");
                        if (dataPoint.Value < 0)
                        {
                            color = SKColor.Parse("#F44336");
                        }

                        entries.Add(new Entry((float)dataPoint.Value)
                        {
                            ValueLabel = dataPoint.Value.ToString("C"),
                            Label = dataPoint.Key.ToString("Y"),
                            Color = color
                        });
                    }
                }
                NetWorthChart = new LineChart() { Entries = entries };
            }
            finally
            {
                IsBusy = false;
                BusyText = string.Empty;
            }
        }
    }
}
