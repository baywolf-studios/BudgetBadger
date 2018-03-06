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

            var payeeReportResult = await _reportLogic.GetPayeeSpendingTotalsReport();
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

            PayeeChart = new DonutChart();
            PayeeChart.Entries = payeeEntries;
        }
    }
}
