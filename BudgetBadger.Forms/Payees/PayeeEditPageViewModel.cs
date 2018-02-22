﻿using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Forms.Enums;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using Prism.Mvvm;
using BudgetBadger.Core.Sync;

namespace BudgetBadger.Forms.Payees
{
    public class PayeeEditPageViewModel : BindableBase, INavigationAware
    {
        readonly IPayeeLogic _payeeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly ISync _syncService;

        bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        string _busyText;
        public string BusyText
        {
            get { return _busyText; }
            set { SetProperty(ref _busyText, value); }
        }

        Payee _payee;
        public Payee Payee
        {
            get { return _payee; }
            set { SetProperty(ref _payee, value); }
        }

        public ICommand SaveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        public PayeeEditPageViewModel(INavigationService navigationService,
                                      IPageDialogService dialogService,
                                      IPayeeLogic payeeLogic,
                                      ISync syncService)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;
            _payeeLogic = payeeLogic;
            _syncService = syncService;

            Payee = new Payee();

            SaveCommand = new DelegateCommand(async () => await ExecuteSaveCommand());
            DeleteCommand = new DelegateCommand(async () => await ExecuteDeleteCommand());
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
            var payee = parameters.GetValue<Payee>(PageParameter.Payee);
            if (payee != null)
            {
                Payee = payee.DeepCopy();
            }
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
        }

        public async Task ExecuteSaveCommand()
        {
            IsBusy = true;

            try
            {
                BusyText = "Saving";
                var result = await _payeeLogic.SavePayeeAsync(Payee);

                if (result.Success)
                {
                    BusyText = "Syncing";
                    var syncResult = await _syncService.FullSync();
                    if (!syncResult.Success)
                    {
                        await _dialogService.DisplayAlertAsync("Sync Unsuccessful", syncResult.Message, "OK");
                    }
                    await _navigationService.GoBackAsync();
                }
                else
                {
                    await _dialogService.DisplayAlertAsync("Error", result.Message, "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteDeleteCommand()
        {
            var result = await _payeeLogic.DeletePayeeAsync(Payee);

            if (result.Success)
            {
                await _navigationService.GoBackToRootAsync();
            }
            else
            {
                await _dialogService.DisplayAlertAsync("Error", result.Message, "Okay");
            }
        }
    }
}
