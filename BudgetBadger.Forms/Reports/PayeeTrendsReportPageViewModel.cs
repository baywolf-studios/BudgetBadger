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
using SkiaSharp;

namespace BudgetBadger.Forms.Reports
{
    public class PayeeTrendsReportPageViewModel: BindableBase, INavigatingAware
    {
        readonly INavigationService _navigationService;
        readonly IReportLogic _reportLogic;
        readonly IPayeeLogic _payeeLogic;

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
            set => SetProperty(ref _beginDate, value);
        }

        DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        Payee _selectedPayee;
        public Payee SelectedPayee
        {
            get => _selectedPayee;
            set => SetProperty(ref _selectedPayee, value);
        }

        IReadOnlyList<Payee> _payees;
        public IReadOnlyList<Payee> Payees
        {
            get => _payees;
            set => SetProperty(ref _payees, value);
        }

        Chart _payeeChart;
        public Chart PayeeChart
        {
            get => _payeeChart;
            set => SetProperty(ref _payeeChart, value);
        }

        public PayeeTrendsReportPageViewModel(INavigationService navigationService, IPayeeLogic payeeLogic, IReportLogic reportLogic)
        {
            _navigationService = navigationService;
            _reportLogic = reportLogic;
            _payeeLogic = payeeLogic;

            Payees = new List<Payee>();

            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());

            var now = DateTime.Now;
            EndDate = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddTicks(-1);
            BeginDate = EndDate.AddMonths(-12);
        }

        public async void OnNavigatingTo(NavigationParameters parameters)
        {
            var payeesResult = await _payeeLogic.GetPayeesAsync();
            if (payeesResult.Success)
            {
                Payees = payeesResult.Data.ToList();
                SelectedPayee = Payees.FirstOrDefault();
            }

            var payee = parameters.GetValue<Payee>(PageParameter.Payee);
            if (payee != null)
            {
                SelectedPayee = Payees.FirstOrDefault(p => p.Id == payee.Id);
            }

            await ExecuteRefreshCommand();
        }

        public async Task ExecuteRefreshCommand()
        {
            if (SelectedPayee == null)
            {
                return;
            }

            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            BusyText = "Loading...";

            try
            {
                var payeeEntries = new List<Entry>();

                var payeeReportResult = await _reportLogic.GetPayeeTrendsReport(SelectedPayee.Id, BeginDate, EndDate);
                if (payeeReportResult.Success)
                {
                    foreach (var datapoint in payeeReportResult.Data)
                    {
                        var color = SKColor.Parse("#4CAF50");
                        if (datapoint.YValue < 0)
                        {
                            color = SKColor.Parse("#F44336");
                        }

                        payeeEntries.Add(new Entry((float)datapoint.YValue)
                        {
                            Label = datapoint.XLabel,
                            ValueLabel = datapoint.YLabel,
                            Color = color
                        });
                    }
                }

                PayeeChart = new PointChart { Entries = payeeEntries };
            }
            finally
            {
                IsBusy = false;
                BusyText = string.Empty;
            }
        }
    }
}

