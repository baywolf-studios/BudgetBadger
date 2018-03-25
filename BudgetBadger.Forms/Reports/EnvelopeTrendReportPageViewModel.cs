using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using Microcharts;
using Prism.AppModel;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using SkiaSharp;

namespace BudgetBadger.Forms.Reports
{
    public class EnvelopeTrendReportsPageViewModel : BindableBase, IPageLifecycleAware
    {
        readonly INavigationService _navigationService;
        readonly IReportLogic _reportLogic;
        readonly IEnvelopeLogic _envelopeLogic;

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

        Envelope _selectedEnvelope;
        public Envelope SelectedEnvelope
        {
            get => _selectedEnvelope;
            set => SetProperty(ref _selectedEnvelope, value);
        }

        IReadOnlyList<Envelope> _envelopes;
        public IReadOnlyList<Envelope> Envelopes
        {
            get => _envelopes;
            set => SetProperty(ref _envelopes, value);
        }

        Chart _envelopeChart;
        public Chart EnvelopeChart
        {
            get => _envelopeChart;
            set => SetProperty(ref _envelopeChart, value);
        }

        public EnvelopeTrendReportsPageViewModel(INavigationService navigationService, IEnvelopeLogic envelopeLogic, IReportLogic reportLogic)
        {
            _navigationService = navigationService;
            _reportLogic = reportLogic;
            _envelopeLogic = envelopeLogic;

            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());

            BeginDate = DateTime.MinValue;
            EndDate = DateTime.MaxValue;
        }

        public async void OnAppearing()
        {
            var envelopesResult = await _envelopeLogic.GetEnvelopesForSelectionAsync();
            if (envelopesResult.Success)
            {
                Envelopes = envelopesResult.Data.ToList();
                SelectedEnvelope = Envelopes.FirstOrDefault();
            }
            else
            {
                //show some error
            }

            await ExecuteRefreshCommand();
        }

        public void OnDisappearing()
        {
        }

        public async Task ExecuteRefreshCommand()
        {
            if (SelectedEnvelope == null)
            {
                return;
            }
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            BusyText = "Loading...";

            try
            {
                var envelopeEntries = new List<Entry>();

                var beginDate = DateRangeFilter ? (DateTime?)BeginDate : null;
                var endDate = DateRangeFilter ? (DateTime?)EndDate : null;

                var envelopeReportResult = await _reportLogic.GetSpendingTrendsByEnvelopeReport(SelectedEnvelope.Id, beginDate, endDate);
                if (envelopeReportResult.Success)
                {
                    foreach (var datapoint in envelopeReportResult.Data)
                    {
                        var color = SKColor.Parse("#4CAF50");
                        if (datapoint.Value < 0)
                        {
                            color = SKColor.Parse("#F44336");
                        }

                        envelopeEntries.Add(new Entry((float)datapoint.Value)
                        {
                            Label = datapoint.Key.ToString("Y"),
                            ValueLabel = datapoint.Value.ToString("C"),
                            Color = color
                        });
                    }
                }

                EnvelopeChart = new BarChart() { Entries = envelopeEntries };
            }
            finally
            {
                IsBusy = false;
                BusyText = string.Empty;
            }
        }
    }
}

