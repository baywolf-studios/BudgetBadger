using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Logic;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Models;
using Microcharts;
using Prism.AppModel;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using SkiaSharp;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Reports
{
    public class EnvelopeTrendsReportsPageViewModel : BindableBase, INavigationAware, IInitializeAsync
    {
        readonly IResourceContainer _resourceContainer;
        readonly INavigationService _navigationService;
        readonly IReportLogic _reportLogic;
        readonly IEnvelopeLogic _envelopeLogic;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
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

        Envelope _selectedEnvelope;
        public Envelope SelectedEnvelope
        {
            get => _selectedEnvelope;
            set
            {
                if (SetProperty(ref _selectedEnvelope, value))
                {
                    RefreshCommand.Execute(null);
                }
            }
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

        bool _noResults;
        public bool NoResults
        {
            get => _noResults;
            set => SetProperty(ref _noResults, value);
        }

        public EnvelopeTrendsReportsPageViewModel(IResourceContainer resourceContainer,
            INavigationService navigationService,
            IEnvelopeLogic envelopeLogic,
            IReportLogic reportLogic)
        {
            _resourceContainer = resourceContainer;
            _navigationService = navigationService;
            _reportLogic = reportLogic;
            _envelopeLogic = envelopeLogic;

            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());

            Envelopes = new List<Envelope>();

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

        public async Task InitializeAsync(INavigationParameters parameters)
        {
            var envelopesResult = await _envelopeLogic.GetEnvelopesForReportAsync();
            if (envelopesResult.Success)
            {
                Envelopes = envelopesResult.Data.ToList();
            }

            var envelope = parameters.GetValue<Envelope>(PageParameter.Envelope);
            if (envelope != null)
            {
                SelectedEnvelope = Envelopes.FirstOrDefault(e => e.Id == envelope.Id);
            }
            else
            {
                SelectedEnvelope = Envelopes.FirstOrDefault();
            }

            var beginDate = parameters.GetValue<DateTime?>(PageParameter.ReportBeginDate);
            if (beginDate.HasValue && beginDate != BeginDate)
            {
                BeginDate = beginDate.GetValueOrDefault();
            }

            var endDate = parameters.GetValue<DateTime?>(PageParameter.ReportEndDate);
            if (endDate.HasValue && endDate != EndDate)
            {
                EndDate = endDate.GetValueOrDefault();
            }
        }

        public async Task ExecuteRefreshCommand()
        {
            if (SelectedEnvelope == null)
            {
                NoResults = true;
                return;
            }

            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            BusyText = _resourceContainer.GetResourceString("BusyTextLoading");

            try
            {
                var envelopeEntries = new List<Microcharts.Entry>();

                await Task.Yield();
                var envelopeReportResult = await _reportLogic.GetEnvelopeTrendsReport(SelectedEnvelope.Id, BeginDate, EndDate);
                if (envelopeReportResult.Success)
                {
                    foreach (var datapoint in envelopeReportResult.Data)
                    {
                        var color = SKColor.Parse(((Color)Application.Current.Resources["SuccessColor"]).GetHexString());
                        if (datapoint.YValue < 0)
                        {
                            color = SKColor.Parse(((Color)Application.Current.Resources["FailureColor"]).GetHexString());
                        }

                        envelopeEntries.Add(new Microcharts.Entry((float)datapoint.YValue)
                        {
                            Label = datapoint.XLabel,
                            ValueLabel = datapoint.YLabel,
                            Color = color
                        });
                    }
                }

                EnvelopeChart = new PointChart { Entries = envelopeEntries };
                NoResults = !envelopeEntries.Any(e => Math.Abs(e.Value) > 0);
            }
            finally
            {
                IsBusy = false;
                BusyText = string.Empty;
            }
        }
    }
}

