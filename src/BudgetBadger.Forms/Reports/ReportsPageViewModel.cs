﻿using System;
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
using BudgetBadger.Core.Settings;
using Prism;

namespace BudgetBadger.Forms.Reports
{
    public class ReportsPageViewModel : BaseViewModel, INavigatedAware
    {
        readonly IResourceContainer _resourceContainer;
        readonly INavigationService _navigationService;
        readonly ISettings _settings;
        readonly IPurchaseService _purchaseService;
        readonly IPageDialogService _dialogService;

        string _netWorthReport;
        string _envelopeSpendingReport;
        string _payeeSpendingReport;
        string _spendingTrendByEnvelopeReport;
        string _spendingTrendByPayeeReport;

        public ICommand ReportCommand { get; set; }
        public ICommand RestoreProCommand { get; set; }
        public ICommand PurchaseProCommand { get; set; }

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

        bool _hasPro;
        public bool HasPro
        {
            get => _hasPro;
            set => SetProperty(ref _hasPro, value);
        }

        KeyValuePair<string, bool> _selectedReport;
        public KeyValuePair<string, bool> SelectedReport
        {
            get => _selectedReport;
            set => SetProperty(ref _selectedReport, value);
        }

        IList<KeyValuePair<string, bool>> _reports;
        public IList<KeyValuePair<string, bool>> Reports
        {
            get => _reports;
            set => SetProperty(ref _reports, value);
        }

        public ReportsPageViewModel(IResourceContainer resourceContainer,
            INavigationService navigationService,
            ISettings settings,
                                    IPageDialogService dialogService, 
                                    IPurchaseService purchaseService)
        {
            _resourceContainer = resourceContainer;
            _navigationService = navigationService;
            _purchaseService = purchaseService;
            _dialogService = dialogService;
            _settings = settings;

            HasPro = false;

            ResetReports();

            RestoreProCommand = new Command(async () => await ExecuteRestoreProCommand());
            PurchaseProCommand = new Command(async () => await ExecutePurchaseProCommand());
            ReportCommand = new Command<object>(async s => await ExecuteReportCommand(s));
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            SelectedReport = new KeyValuePair<string, bool>();
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
        }

        public override async void OnActivated()
        {
            var purchasedPro = await _purchaseService.VerifyPurchaseAsync(Purchases.Pro);
            HasPro = purchasedPro.Success;
            ResetReports();
        }

        void ResetReports()
        {
            _netWorthReport = _resourceContainer.GetResourceString("NetWorthReportPageTitle");
            _envelopeSpendingReport = _resourceContainer.GetResourceString("EnvelopeSpendingReportPageTitle");
            _payeeSpendingReport = _resourceContainer.GetResourceString("PayeeSpendingReportPageTitle");
            _spendingTrendByEnvelopeReport = _resourceContainer.GetResourceString("EnvelopeTrendsReportPageTitle");
            _spendingTrendByPayeeReport = _resourceContainer.GetResourceString("PayeeTrendsReportPageTitle");

            Reports = new List<KeyValuePair<string, bool>>
            {
                new KeyValuePair<string, bool>(_netWorthReport, HasPro),
                new KeyValuePair<string, bool>(_envelopeSpendingReport, HasPro),
                new KeyValuePair<string, bool>(_payeeSpendingReport, HasPro),
                new KeyValuePair<string, bool>(_spendingTrendByEnvelopeReport, HasPro),
                new KeyValuePair<string, bool>(_spendingTrendByPayeeReport, HasPro)
            };
        }

        public async Task ExecuteRestoreProCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            BusyText = _resourceContainer.GetResourceString("BusyTextLoading");

            try
            {
                var result = await _purchaseService.RestorePurchaseAsync(Purchases.Pro);

                HasPro = result.Success;

                if (!HasPro)
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertRestorePurchaseUnsuccessful"),
                        result.Message,
                        _resourceContainer.GetResourceString("AlertOk"));
                }
            }
            finally
            {
                ResetReports();
                IsBusy = false;
            }
        }

        public async Task ExecutePurchaseProCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            BusyText = _resourceContainer.GetResourceString("BusyTextLoading");

            try
            {
                if (!HasPro)
                {
                    var purchaseResult = await _purchaseService.PurchaseAsync(Purchases.Pro);

                    HasPro = purchaseResult.Success;

                    if (!HasPro)
                    {
                        await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertPurchaseUnsuccessful"),
                            purchaseResult.Message,
                            _resourceContainer.GetResourceString("AlertOk"));
                    }
                }
            }
            finally
            {
                ResetReports();
                IsBusy = false;
            }
        }

        public async Task ExecuteReportCommand(object obj)
        {
            if (obj != null 
                && obj is KeyValuePair<string, bool> report)
            {
                if (HasPro
                    && report.Key != null)
                {
                    if (report.Key == _netWorthReport)
                    {
                        await _navigationService.NavigateAsync(PageName.NetWorthReportPage);
                    }
                    else if (report.Key == _envelopeSpendingReport)
                    {
                        await _navigationService.NavigateAsync(PageName.EnvelopesSpendingReportPage);
                    }
                    else if (report.Key == _payeeSpendingReport)
                    {
                        await _navigationService.NavigateAsync(PageName.PayeesSpendingReportPage);
                    }
                    else if (report.Key == _spendingTrendByEnvelopeReport)
                    {
                        await _navigationService.NavigateAsync(PageName.EnvelopeTrendsReportPage);
                    }
                    else if (report.Key == _spendingTrendByPayeeReport)
                    {
                        await _navigationService.NavigateAsync(PageName.PayeeTrendsReportPage);
                    }
                }
            }
        }
    }
}