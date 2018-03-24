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
            var envelopeEntries = new List<Entry>();

            var envelopeReportResult = await _reportLogic.GetEnvelopeSpendingTotalsReport(BeginDate, EndDate);
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

            EnvelopeChart = new DonutChart() { Entries = envelopeEntries };
        }
    }
}
