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
using System.Threading.Tasks;
using BudgetBadger.Forms.Enums;

namespace BudgetBadger.Forms.Reports
{
    public class ReportsPageViewModel : BindableBase
    {
        readonly INavigationService _navigationService;
        readonly string _netWorthReport = "Net Worth";
        readonly string _envelopeSpendingReport = "Envelopes Spending";
        readonly string _payeeSpendingReport = "Payees Spending";
        readonly string _spendingTrendByEnvelopeReport = "Envelope Trends";
        readonly string _spendingTrendByPayeeReport = "Payee Trends";

        public ICommand ReportCommand { get; set; }

        public ICommand NetWorthCommand { get; set; }
        public ICommand EnvelopesSpendingCommand { get; set; }
        public ICommand PayeesSpendingCommand { get; set; }
        public ICommand EnvelopeTrendCommand { get; set; }
        public ICommand PayeeTrendCommand { get; set; }

        string _selectedReport;
        public string SelectedReport
        {
            get => _selectedReport;
            set => SetProperty(ref _selectedReport, value);
        }

        IList<string> _reports;
        public IList<string> Reports
        {
            get => _reports;
            set => SetProperty(ref _reports, value);
        }

        public ReportsPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            Reports = new List<string>
            {
                _netWorthReport,
                _envelopeSpendingReport,
                _payeeSpendingReport,
                _spendingTrendByEnvelopeReport,
                _spendingTrendByPayeeReport
            };

            ReportCommand = new DelegateCommand<string>(async s => await ExecuteReportCommand(s));
            NetWorthCommand = new DelegateCommand(async () => await ExecuteNetWorthCommand());
            EnvelopesSpendingCommand = new DelegateCommand(async () => await ExecuteEnvelopesSpendingCommand());
            PayeesSpendingCommand = new DelegateCommand(async () => await ExecutePayeesSpendingCommand());
            EnvelopeTrendCommand = new DelegateCommand(async () => await ExecuteEnvelopeTrendCommand());
            PayeeTrendCommand = new DelegateCommand(async () => await ExecutePayeeTrendCommand());
        }

        public async Task ExecuteReportCommand(string report)
        {
            if (report == null)
            {
                return;
            }

            if (report == _netWorthReport)
            {
                await _navigationService.NavigateAsync(PageName.NetWorthReportPage);
            }
            else if (report == _envelopeSpendingReport)
            {
                await _navigationService.NavigateAsync(PageName.EnvelopesSpendingReportPage);
            }
            else if (report == _payeeSpendingReport)
            {
                await _navigationService.NavigateAsync(PageName.PayeesSpendingReportPage);
            }
            else if (report == _spendingTrendByEnvelopeReport)
            {
                await _navigationService.NavigateAsync(PageName.EnvelopeTrendsReportPage);
            }
            else if (report == _spendingTrendByPayeeReport)
            {
                await _navigationService.NavigateAsync(PageName.PayeeTrendsReportPage);
            }

            Reports = new List<string>
            {
                _netWorthReport,
                _envelopeSpendingReport,
                _payeeSpendingReport,
                _spendingTrendByEnvelopeReport,
                _spendingTrendByPayeeReport
            };
        }

        public async Task ExecuteNetWorthCommand()
        {
            await _navigationService.NavigateAsync(PageName.NetWorthReportPage);
        }

        public async Task ExecuteEnvelopesSpendingCommand()
        {
            await _navigationService.NavigateAsync(PageName.EnvelopesSpendingReportPage);
        }

        public async Task ExecutePayeesSpendingCommand()
        {
            await _navigationService.NavigateAsync(PageName.PayeesSpendingReportPage);
        }

        public async Task ExecuteEnvelopeTrendCommand()
        {
            await _navigationService.NavigateAsync(PageName.EnvelopeTrendsReportPage);
        }

        public async Task ExecutePayeeTrendCommand()
        {
            await _navigationService.NavigateAsync(PageName.PayeeTrendsReportPage);
        }
    }
}
