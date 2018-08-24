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
            set => SetProperty(ref _beginDate, value);
        }

        DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
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
            SelectedCommand = new DelegateCommand(async () => await ExecuteSelectedCommand());

            var now = DateTime.Now;
            EndDate = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddTicks(-1);
            BeginDate = EndDate.AddMonths(-12);
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            await ExecuteRefreshCommand();
        }

        public async Task ExecuteRefreshCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

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

        public async Task ExecuteSelectedCommand()
        {
            if (SelectedEnvelope == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { PageParameter.Envelope, SelectedEnvelope.XValue },
                { PageParameter.ReportBeginDate, BeginDate },
                { PageParameter.ReportEndDate, EndDate }
            };
            await _navigationService.NavigateAsync(PageName.EnvelopeTrendsReportPage, parameters);

            SelectedEnvelope = null;
        }
    }
}
