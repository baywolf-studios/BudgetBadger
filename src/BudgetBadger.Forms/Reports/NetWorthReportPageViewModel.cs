using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Logic;
using BudgetBadger.Forms.Style;
using BudgetBadger.Core.Models;
using Microcharts;
using Prism.AppModel;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using SkiaSharp;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Reports
{
    public class NetWorthReportPageViewModel : ObservableBase, INavigationAware
    {
        readonly IResourceContainer _resourceContainer;
        readonly INavigationService _navigationService;
        readonly IReportLogic _reportLogic;

        public ICommand BackCommand { get => new Command(async () => await _navigationService.GoBackAsync()); }
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

        DateTime _beginDate;
        public DateTime BeginDate
        {
            get => _beginDate;
            set
            {
                if (SetProperty(ref _beginDate, value))
                {
                    RefreshCommand.Execute(null);
                }
            }
        }

        DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                {
                    RefreshCommand.Execute(null);
                }
            }
        }

        Chart _netWorthChart;
        public Chart NetWorthChart
        {
            get => _netWorthChart;
            set => SetProperty(ref _netWorthChart, value);
        }

        bool _noResults;
        public bool NoResults
        {
            get => _noResults;
            set => SetProperty(ref _noResults, value);
        }

        public NetWorthReportPageViewModel(IResourceContainer resourceContainer,
            INavigationService navigationService,
            IReportLogic reportLogic)
        {
            _resourceContainer = resourceContainer;
            _navigationService = navigationService;
            _reportLogic = reportLogic;

            RefreshCommand = new Command(async () => await ExecuteRefreshCommand());

            var now = DateTime.Now;
            _endDate = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddTicks(-1);
            if (Xamarin.Forms.Device.Idiom == Xamarin.Forms.TargetIdiom.Desktop || Xamarin.Forms.Device.Idiom == Xamarin.Forms.TargetIdiom.Tablet)
            {
                _beginDate = EndDate.AddMonths(-12);
            }
            else if (Xamarin.Forms.Device.Idiom == Xamarin.Forms.TargetIdiom.Phone)
            {
                _beginDate = EndDate.AddMonths(-6);
            }
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            await ExecuteRefreshCommand();
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public async Task ExecuteRefreshCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            BusyText = _resourceContainer.GetResourceString("BusyTextLoading");

            try
            {
                var entries = new List<Microcharts.Entry>();

                var netWorthReportResult = await _reportLogic.GetNetWorthReport(BeginDate, EndDate);
                if (netWorthReportResult.Success)
                {
                    foreach (var dataPoint in netWorthReportResult.Data)
                    {
                        var color = SKColor.Parse(((Color)DynamicResourceProvider.Instance["green_600"]).GetHexString());
                        if (dataPoint.YValue < 0)
                        {
                            color = SKColor.Parse(((Color)DynamicResourceProvider.Instance["red_600"]).GetHexString());
                        }

                        entries.Add(new Microcharts.Entry((float)dataPoint.YValue)
                        {
                            ValueLabel = dataPoint.YLabel,
                            Label = dataPoint.XLabel,
                            Color = color
                        });
                    }
                }
                NetWorthChart = new LineChart() { LabelTextSize = 24, BackgroundColor = SKColors.Transparent, Entries = entries };
                NoResults = !entries.Any(e => Math.Abs(e.Value) > 0);
            }
            finally
            {
                IsBusy = false;
                BusyText = string.Empty;
            }
        }
    }
}
