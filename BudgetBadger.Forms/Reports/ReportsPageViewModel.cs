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

        public ICommand NetWorthCommand { get; set; }
        public ICommand EnvelopesSpendingCommand { get; set; }
        public ICommand PayeesSpendingCommand { get; set; }
        public ICommand EnvelopeTrendCommand { get; set; }
        public ICommand PayeeTrendCommand { get; set; }

        public ReportsPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            NetWorthCommand = new DelegateCommand(async () => await ExecuteNetWorthCommand());
            EnvelopesSpendingCommand = new DelegateCommand(async () => await ExecuteEnvelopesSpendingCommand());
            PayeesSpendingCommand = new DelegateCommand(async () => await ExecutePayeesSpendingCommand());
            EnvelopeTrendCommand = new DelegateCommand(async () => await ExecuteEnvelopeTrendCommand());
            PayeeTrendCommand = new DelegateCommand(async () => await ExecutePayeeTrendCommand());
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
