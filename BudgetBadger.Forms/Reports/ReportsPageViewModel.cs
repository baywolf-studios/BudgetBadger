using System;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Microcharts;
using System.Collections.Generic;
using SkiaSharp;
using Prism.AppModel;
using BudgetBadger.Core.Logic;
using System.Linq;

namespace BudgetBadger.Forms.Reports
{
    public class ReportsPageViewModel : BindableBase, IPageLifecycleAware
    {
        readonly INavigationService _navigationService;
        readonly IReportLogic _reportLogic;

        public ICommand SyncCommand { get; set; }

        string _syncMode;
        public string SyncMode
        {
            get => _syncMode;
            set => SetProperty(ref _syncMode, value);
        }

        Chart _netWorthChart;
        public Chart NetWorthChart
        {
            get => _netWorthChart;
            set => SetProperty(ref _netWorthChart, value);
        }

        Chart _envelopeChart;
        public Chart EnvelopeChart
        {
            get => _envelopeChart;
            set => SetProperty(ref _envelopeChart, value);
        }

        public ReportsPageViewModel(INavigationService navigationService, IReportLogic reportLogic)
        {
            _navigationService = navigationService;
            _reportLogic = reportLogic;

            NetWorthChart = new LineChart();
            NetWorthChart.Entries = new[]
            {
                new Entry(200)
                {
                    Label = "January",
                    ValueLabel = "200",
                    Color = SKColor.Parse("#266489")
                },
                new Entry(400)
                {
                    Label = "February",
                    ValueLabel = "400",
                    Color = SKColor.Parse("#68B9C0")
                },
                new Entry(-100)
                {
                    Label = "March",
                    ValueLabel = "-100",
                    Color = SKColor.Parse("#90D585")
                }
            };

            //SyncCommand = new DelegateCommand(async () => await ExecuteSyncCommand());
        }

        public async void OnAppearing()
        {
            var entries = new List<Entry>();

            var netWorthReportResult = await _reportLogic.GetNetWorthReport();
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
            NetWorthChart = new LineChart();
            NetWorthChart.Entries = entries;

            var envelopeEntries = new List<Entry>();

            var envelopeReportResult = await _reportLogic.GetSpendingByEnvelopeReport();
            if (envelopeReportResult.Success)
            {
                foreach (var datapoint in envelopeReportResult.Data)
                {
                    envelopeEntries.Add(new Entry((float)datapoint.Value)
                    {
                        Label = datapoint.Key,
                        ValueLabel = datapoint.Value.ToString("C")
                    });
                }
            }

            EnvelopeChart = new DonutChart();
            EnvelopeChart.Entries = envelopeEntries;
        }

        public void OnDisappearing()
        {
        }
    }
}
