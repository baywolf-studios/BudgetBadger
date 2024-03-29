﻿using System;
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
using Prism.Services;
using SkiaSharp;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Reports
{
    public class PayeesSpendingReportPageViewModel : ObservableBase, INavigationAware
    {
        readonly IResourceContainer _resourceContainer;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly IReportLogic _reportLogic;

        public ICommand BackCommand { get => new Command(async () => await _navigationService.GoBackAsync()); }
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

        IReadOnlyList<DataPoint<Payee, decimal>> _payees;
        public IReadOnlyList<DataPoint<Payee, decimal>> Payees
        {
            get => _payees;
            set => SetProperty(ref _payees, value);
        }

        DataPoint<Payee, decimal> _selectedPayee;
        public DataPoint<Payee, decimal> SelectedPayee
        {
            get => _selectedPayee;
            set => SetProperty(ref _selectedPayee, value);
        }

        bool _noResults;
        public bool NoResults
        {
            get => _noResults;
            set => SetProperty(ref _noResults, value);
        }

        public PayeesSpendingReportPageViewModel(IResourceContainer resourceContainer,
            INavigationService navigationService,
                                                 IPageDialogService dialogService,
                                                 IReportLogic reportLogic)
        {
            _resourceContainer = resourceContainer;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _reportLogic = reportLogic;

            RefreshCommand = new Command(async () => await ExecuteRefreshCommand());
            SelectedCommand = new Command<DataPoint<Payee, decimal>>(async d => await ExecuteSelectedCommand(d));

            Payees = new List<DataPoint<Payee, decimal>>();

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
            SelectedPayee = null;
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
                var payeeEntries = new List<Microcharts.Entry>();

                await Task.Yield();
                var payeeReportResult = await _reportLogic.GetPayeesSpendingReport(BeginDate, EndDate);
                if (payeeReportResult.Success)
                {
                    Payees = payeeReportResult.Data;
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertRefreshUnsuccessful"), payeeReportResult.Message, _resourceContainer.GetResourceString("AlertOk"));
                }

                NoResults = !Payees.Any();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteSelectedCommand(DataPoint<Payee, decimal> dataPoint)
        {
            if (dataPoint == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { PageParameter.Payee, dataPoint.XValue },
                { PageParameter.ReportBeginDate, BeginDate },
                { PageParameter.ReportEndDate, EndDate }
            };
            await _navigationService.NavigateAsync(PageName.PayeeTrendsReportPage, parameters);
        }
    }
}
