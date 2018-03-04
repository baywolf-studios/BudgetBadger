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

            var netWorthReportData = await _reportLogic.GetNetWorthReport();

            foreach (var dataPoint in netWorthReportData)
            {
                entries.Add(new Entry((float)dataPoint.Value) { ValueLabel = dataPoint.Value.ToString("C"), Label = dataPoint.Key });
            }

            Chart = new LineChart();
            Chart.Entries = entries;
        }

        public void OnDisappearing()
        {
        }
    }
}
