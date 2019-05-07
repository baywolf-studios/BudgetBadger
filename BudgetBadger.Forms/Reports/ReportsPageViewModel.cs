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
using BudgetBadger.Core.Purchase;
using Prism.Services;
using Xamarin.Forms;
using BudgetBadger.Core.LocalizedResources;

namespace BudgetBadger.Forms.Reports
{
    public class ReportsPageViewModel : BindableBase
    {
        readonly IResourceContainer _resourceContainer;
        readonly INavigationService _navigationService;
        readonly IPurchaseService _purchaseService;
        readonly IPageDialogService _dialogService;

        string _netWorthReport;
        string _envelopeSpendingReport;
        string _payeeSpendingReport;
        string _spendingTrendByEnvelopeReport;
        string _spendingTrendByPayeeReport;

        public ICommand ReportCommand { get; set; }

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

        public ReportsPageViewModel(IResourceContainer resourceContainer,
            INavigationService navigationService,
                                    IPageDialogService dialogService, 
                                    IPurchaseService purchaseService)
        {
            _resourceContainer = resourceContainer;
            _navigationService = navigationService;
            _purchaseService = purchaseService;
            _dialogService = dialogService;

            ResetReports();

            ReportCommand = new DelegateCommand<string>(async s => await ExecuteReportCommand(s));
        }

        void ResetReports()
        {
            _netWorthReport = _resourceContainer.GetResourceString("NetWorthReportPageTitle");
            _envelopeSpendingReport = _resourceContainer.GetResourceString("EnvelopeSpendingReportPageTitle");
            _payeeSpendingReport = _resourceContainer.GetResourceString("PayeeSpendingReportPageTitle");
            _spendingTrendByEnvelopeReport = _resourceContainer.GetResourceString("EnvelopeTrendsReportPageTitle");
            _spendingTrendByPayeeReport = _resourceContainer.GetResourceString("PayeeTrendsReportPageTitle");

            Reports = new List<string>
            {
                _netWorthReport,
                _envelopeSpendingReport,
                _payeeSpendingReport,
                _spendingTrendByEnvelopeReport,
                _spendingTrendByPayeeReport
            };
        }

        public async Task ExecuteReportCommand(string report)
        {
            if (report == null)
            {
                return;
            }

            var allowedReports = await _purchaseService.VerifyPurchaseAsync(Purchases.Pro);
            if (!allowedReports.Success)
            {
                // show some dialog asking if they would like to purchase
                var wantToPurchase = await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertBudgetBadgerPro"),
                    _resourceContainer.GetResourceString("AlertMessageBudgetBadgerPro"),
                    _resourceContainer.GetResourceString("AlertPurchase"),
                    _resourceContainer.GetResourceString("AlertCancel"));

                if (wantToPurchase)
                {
                    var purchaseResult = await _purchaseService.PurchaseAsync(Purchases.Pro);
                    if (!purchaseResult.Success)
                    {
                        //show dialog of not allowing
                        await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertNotPurchased"), purchaseResult.Message, _resourceContainer.GetResourceString("AlertOk"));
                        ResetReports();
                        return;
                    }
                }
                else
                {
                    ResetReports();
                    return;
                }

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

            ResetReports();
        }
    }
}
