using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Models;
using Microcharts;
using Prism.AppModel;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using SkiaSharp;

namespace BudgetBadger.Forms.Reports
{
    public class EnvelopesSpendingReportPageViewModel : BindableBase, INavigatingAware
    {
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly IReportLogic _reportLogic;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand RefreshCommand { get; set; }
        public ICommand SelectedCommand { get; set; }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
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

        IReadOnlyList<DataPoint<Envelope, decimal>> _envelopes;
        public IReadOnlyList<DataPoint<Envelope, decimal>> Envelopes
        {
            get => _envelopes;
            set => SetProperty(ref _envelopes, value);
        }

        DataPoint<Envelope, decimal> _selectedEnvelope;
        public DataPoint<Envelope, decimal> SelectedEnvelope
        {
            get => _selectedEnvelope;
            set => SetProperty(ref _selectedEnvelope, value);
        }

        public EnvelopesSpendingReportPageViewModel(INavigationService navigationService,
                                                    IPageDialogService dialogService,
                                                    IReportLogic reportLogic)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            _reportLogic = reportLogic;

            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            SelectedCommand = new DelegateCommand<DataPoint<Envelope, decimal>>(async d => await ExecuteSelectedCommand(d));

            var now = DateTime.Now;
            EndDate = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddTicks(-1);
            if (Xamarin.Forms.Device.Idiom == Xamarin.Forms.TargetIdiom.Desktop || Xamarin.Forms.Device.Idiom == Xamarin.Forms.TargetIdiom.Tablet)
            {
                BeginDate = EndDate.AddMonths(-12);
            }
            else if (Xamarin.Forms.Device.Idiom == Xamarin.Forms.TargetIdiom.Phone)
            {
                BeginDate = EndDate.AddMonths(-6);
            }
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            await ExecuteRefreshCommand();
        }

        public async Task ExecuteRefreshCommand()
        {
            if (!IsBusy)
            {
                IsBusy = true;
            }

            try
            {
                var envelopeEntries = new List<Entry>();

                var envelopeReportResult = await _reportLogic.GetEnvelopesSpendingReport(BeginDate, EndDate);
                if (envelopeReportResult.Success)
                {
                    Envelopes = envelopeReportResult.Data;
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("Error", envelopeReportResult.Message, "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteSelectedCommand(DataPoint<Envelope, decimal> dataPoint)
        {
            if (dataPoint == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { PageParameter.Envelope, dataPoint.XValue },
                { PageParameter.ReportBeginDate, BeginDate },
                { PageParameter.ReportEndDate, EndDate }
            };
            await _navigationService.NavigateAsync(PageName.EnvelopeTrendsReportPage, parameters);
        }
    }
}
