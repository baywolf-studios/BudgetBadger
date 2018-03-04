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

        Chart _chart;
        public Chart Chart
        {
            get => _chart;
            set => SetProperty(ref _chart, value);
        }

        public ReportsPageViewModel(INavigationService navigationService, IReportLogic reportLogic)
        {
            _navigationService = navigationService;
            _reportLogic = reportLogic;

            Chart = new LineChart();
            Chart.Entries = new[]
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
                foreach (var dataPoint in netWorthReportResult.Data.OrderBy(d => d.X))
                {
                    var color = SKColor.Parse("#4CAF50");
                    if (dataPoint.Y < 0)
                    {
                        color = SKColor.Parse("#F44336");
                    }

                    entries.Add(new Entry((float)dataPoint.Y)
                    {
                        ValueLabel = dataPoint.YLabel,
                        Label = dataPoint.XLabel,
                        Color = color
                    });
                }
            }
            Chart = new LineChart();
            Chart.Entries = entries;
        }

        public void OnDisappearing()
        {
        }
    }
}
