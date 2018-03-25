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
        readonly string _spendingTrendByEnvelopeReport = "Spending Trend By Envelope";
        readonly string _spendingTrendByPayeeReport = "Spending Trend By Payee";

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

            ReportCommand = new DelegateCommand(async () => await ExecuteReportCommand());
            NetWorthCommand = new DelegateCommand(async () => await ExecuteNetWorthCommand());
            EnvelopesSpendingCommand = new DelegateCommand(async () => await ExecuteEnvelopesSpendingCommand());
            PayeesSpendingCommand = new DelegateCommand(async () => await ExecutePayeesSpendingCommand());
            EnvelopeTrendCommand = new DelegateCommand(async () => await ExecuteEnvelopeTrendCommand());
            PayeeTrendCommand = new DelegateCommand(async () => await ExecutePayeeTrendCommand());
        }

        public async Task ExecuteReportCommand()
        {
            if (SelectedReport == null)
            {
                return;
            }

            if (SelectedReport == _netWorthReport)
            {
                await _navigationService.NavigateAsync(PageName.NetWorthReportPage);
            }
            else if (SelectedReport == _envelopeSpendingReport)
            {
                await _navigationService.NavigateAsync(PageName.EnvelopesSpendingReportPage);
            }
            else if (SelectedReport == _payeeSpendingReport)
            {
                await _navigationService.NavigateAsync(PageName.PayeesSpendingReportPage);
            }
            else if (SelectedReport == _spendingTrendByEnvelopeReport)
            {
                await _navigationService.NavigateAsync(PageName.EnvelopesSpendingReportPage);
            }
            else if (SelectedReport == _spendingTrendByPayeeReport)
            {
                await _navigationService.NavigateAsync(PageName.PayeesSpendingReportPage);
            }

            SelectedReport = null;
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
            await _navigationService.NavigateAsync(PageName.EnvelopeTrendReportPage);
        }

        public async Task ExecutePayeeTrendCommand()
        {
            await _navigationService.NavigateAsync(PageName.PayeeTrendReportPage);
        }
    }
}
