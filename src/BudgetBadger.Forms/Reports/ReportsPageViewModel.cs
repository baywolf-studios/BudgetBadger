using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Settings;
using BudgetBadger.Forms.Enums;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Reports
{
    public class ReportsPageViewModel : BaseViewModel, INavigatedAware
    {
        readonly IResourceContainer _resourceContainer;
        readonly INavigationService _navigationService;

        string _netWorthReport;
        string _envelopeSpendingReport;
        string _payeeSpendingReport;
        string _spendingTrendByEnvelopeReport;
        string _spendingTrendByPayeeReport;

        public ICommand ReportCommand { get; set; }

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
                                    INavigationService navigationService)
        {
            _resourceContainer = resourceContainer;
            _navigationService = navigationService;

            ResetReports();

            ReportCommand = new Command<object>(async s => await ExecuteReportCommand(s));
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            SelectedReport = null;
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
        }

        public override void OnActivated()
        {
            ResetReports();
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

        public async Task ExecuteReportCommand(object obj)
        {
            if (obj != null 
                && obj is string report)
            {
                if (report != null)
                {
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
                }
            }
        }
    }
}
