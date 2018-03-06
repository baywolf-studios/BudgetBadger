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
    public class EnvelopesSpendingReportsPageViewModel : BindableBase, IPageLifecycleAware
    {
        readonly INavigationService _navigationService;
        readonly IReportLogic _reportLogic;

        public ICommand RefreshCommand { get; set; }

        Chart _envelopeChart;
        public Chart EnvelopeChart
        {
            get => _envelopeChart;
            set => SetProperty(ref _envelopeChart, value);
        }

        public EnvelopesSpendingReportsPageViewModel(INavigationService navigationService, IReportLogic reportLogic)
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
            var envelopeEntries = new List<Entry>();

            var envelopeReportResult = await _reportLogic.GetEnvelopeSpendingTotalsReport();
            if (envelopeReportResult.Success)
            {
                foreach (var datapoint in envelopeReportResult.Data)
                {
                    envelopeEntries.Add(new Entry((float)datapoint.Value)
                    {
                        Label = datapoint.Key,
                        ValueLabel = datapoint.Value.ToString("C"),
                        Color = SKColor.Parse("#4CAF50")
                    });
                }
            }

            EnvelopeChart = new DonutChart();
            EnvelopeChart.Entries = envelopeEntries;
        }
    }
}
