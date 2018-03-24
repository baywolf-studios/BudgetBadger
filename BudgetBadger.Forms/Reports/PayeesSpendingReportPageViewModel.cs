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

        DateTime _beginDate;
        public DateTime BeginDate
        {
            get => _beginDate;
            set { SetProperty(ref _beginDate, value); RefreshCommand.Execute(null); }
        }

        DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set { SetProperty(ref _endDate, value); RefreshCommand.Execute(null); }
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
            var payeeEntries = new List<Entry>();

            var payeeReportResult = await _reportLogic.GetPayeeSpendingTotalsReport(BeginDate, EndDate);
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
    }
}
