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
using Xamarin.Forms;

namespace BudgetBadger.Forms.Reports
{
    public class PayeeTrendsReportPageViewModel: BindableBase, INavigatingAware
    {
        readonly INavigationService _navigationService;
        readonly IReportLogic _reportLogic;
        readonly IPayeeLogic _payeeLogic;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
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

        Payee _selectedPayee;
        public Payee SelectedPayee
        {
            get => _selectedPayee;
            set
            {
                if (SetProperty(ref _selectedPayee, value))
                {
                    RefreshCommand.Execute(null);
                }
            }
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

            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());

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
            var payeesResult = await _payeeLogic.GetPayeesForReportAsync();
            if (payeesResult.Success)
            {
                Payees = payeesResult.Data.ToList();
            }

            var payee = parameters.GetValue<Payee>(PageParameter.Payee);
            if (payee != null)
            {
                SelectedPayee = Payees.FirstOrDefault(p => p.Id == payee.Id);
            }
            else
            {
                SelectedPayee = Payees.FirstOrDefault();
            }

            var beginDate = parameters.GetValue<DateTime?>(PageParameter.ReportBeginDate);
            if (beginDate.HasValue)
            {
                BeginDate = beginDate.GetValueOrDefault();
            }

            var endDate = parameters.GetValue<DateTime?>(PageParameter.ReportEndDate);
            if (endDate.HasValue)
            {
                EndDate = endDate.GetValueOrDefault();
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
                var payeeEntries = new List<Microcharts.Entry>();

                var payeeReportResult = await _reportLogic.GetPayeeTrendsReport(SelectedPayee.Id, BeginDate, EndDate);
                if (payeeReportResult.Success)
                {
                    foreach (var datapoint in payeeReportResult.Data)
                    {
                        var color = SKColor.Parse(((Color)Application.Current.Resources["SuccessColor"]).GetHexString());
                        if (datapoint.YValue < 0)
                        {
                            color = SKColor.Parse(((Color)Application.Current.Resources["FailureColor"]).GetHexString());
                        }

                        payeeEntries.Add(new Microcharts.Entry((float)datapoint.YValue)
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

