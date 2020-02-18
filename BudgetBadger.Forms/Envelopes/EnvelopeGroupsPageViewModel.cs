﻿using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;
using BudgetBadger.Forms.Enums;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Collections.Generic;
using System.Linq;
using Prism.Mvvm;
using Prism.AppModel;
using BudgetBadger.Core.Sync;
using Prism;
using System.Collections.ObjectModel;
using BudgetBadger.Models.Extensions;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Core.Purchase;

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopeGroupsPageViewModel : BindableBase, INavigationAware, IInitializeAsync
    {
        readonly Lazy<IResourceContainer> _resourceContainer;
        readonly Lazy<IEnvelopeLogic> _envelopeGroupLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly Lazy<ISyncFactory> _syncFactory;
        readonly Lazy<IPurchaseService> _purchaseService;

        public ICommand BackCommand { get => new DelegateCommand(async () => await _navigationService.GoBackAsync()); }
        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand RefreshEnvelopeGroupCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand SaveSearchCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public Predicate<object> Filter { get => (envelopeGroup) => _envelopeGroupLogic.Value.FilterEnvelopeGroup((EnvelopeGroup)envelopeGroup, SearchText); }

        bool _needToSync;

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        ObservableList<EnvelopeGroup> _envelopeGroups;
        public ObservableList<EnvelopeGroup> EnvelopeGroups
        {
            get => _envelopeGroups;
            set => SetProperty(ref _envelopeGroups, value);
        }

        EnvelopeGroup _selectedEnvelopeGroup;
        public EnvelopeGroup SelectedEnvelopeGroup
        {
            get => _selectedEnvelopeGroup;
            set => SetProperty(ref _selectedEnvelopeGroup, value);
        }

        public bool HasSearchText { get => !string.IsNullOrWhiteSpace(SearchText); }

        string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { SetProperty(ref _searchText, value); RaisePropertyChanged(nameof(HasSearchText)); }
        }

        bool _noEnvelopeGroups;
        public bool NoEnvelopeGroups
        {
            get => _noEnvelopeGroups;
            set => SetProperty(ref _noEnvelopeGroups, value);
        }

        bool _hasPro;
        public bool HasPro
        {
            get => _hasPro;
            set => SetProperty(ref _hasPro, value);
        }

        public EnvelopeGroupsPageViewModel(Lazy<IResourceContainer> resourceContainer,
                                   INavigationService navigationService,
                                   IPageDialogService dialogService,
                                   Lazy<IEnvelopeLogic> envelopeGroupLogic,
                                   Lazy<ISyncFactory> syncFactory,
                                   Lazy<IPurchaseService> purchaseService)
        {
            _resourceContainer = resourceContainer;
            _envelopeGroupLogic = envelopeGroupLogic;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _syncFactory = syncFactory;
            _purchaseService = purchaseService;

            EnvelopeGroups = new ObservableList<EnvelopeGroup>();
            SelectedEnvelopeGroup = null;

            SelectedCommand = new DelegateCommand<EnvelopeGroup>(async p => await ExecuteSelectedCommand(p));
            RefreshCommand = new DelegateCommand(async () => await ExecuteRefreshCommand());
            SaveSearchCommand = new DelegateCommand(async () => await ExecuteSaveSearchCommand());
            SaveCommand = new DelegateCommand<EnvelopeGroup>(async p => await ExecuteSaveCommand(p));
            AddCommand = new DelegateCommand(async () => await ExecuteAddCommand());
            EditCommand = new DelegateCommand<EnvelopeGroup>(async a => await ExecuteEditCommand(a));
        }

        public async void OnNavigatedFrom(INavigationParameters parameters)
        {
            SelectedEnvelopeGroup = null;

            if (_needToSync)
            {
                var syncService = _syncFactory.Value.GetSyncService();
                var syncResult = await syncService.FullSync();

                if (syncResult.Success)
                {
                    await _syncFactory.Value.SetLastSyncDateTime(DateTime.Now);
                    _needToSync = false;
                }
            }
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.TryGetValue(PageParameter.EnvelopeGroup, out EnvelopeGroup envelopeGroup))
            {
                await ExecuteRefreshEnvelopeGroupCommand(envelopeGroup);
            }

            if (parameters.TryGetValue(PageParameter.Transaction, out Transaction transaction))
            {
                await ExecuteRefreshEnvelopeGroupCommand(transaction.Envelope.Group);
            }
        }

        public async Task InitializeAsync(INavigationParameters parameters)
        {
            var purchasedPro = await _purchaseService.Value.VerifyPurchaseAsync(Purchases.Pro);
            HasPro = purchasedPro.Success;

            await ExecuteRefreshCommand();
        }

        public async Task ExecuteSelectedCommand(EnvelopeGroup envelopeGroup)
        {
            if (envelopeGroup == null)
            {
                return;
            }

            if (envelopeGroup.IsGenericHiddenEnvelopeGroup)
            {
                await _navigationService.NavigateAsync(PageName.HiddenEnvelopeGroupsPage);
            }
            else
            { 
                var parameters = new NavigationParameters
                {
                    { PageParameter.EnvelopeGroup, envelopeGroup }
                };

                await _navigationService.NavigateAsync(PageName.EnvelopeGroupEditPage, parameters);
            }
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
                var result = await _envelopeGroupLogic.Value.GetEnvelopeGroupsAsync();

                if (result.Success)
                {
                    EnvelopeGroups.ReplaceRange(result.Data);
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertRefreshUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
                }

                NoEnvelopeGroups = (EnvelopeGroups?.Count ?? 0) == 0;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteRefreshEnvelopeGroupCommand(EnvelopeGroup envelopeGroup)
        {
            var updatedEnvelopeGroup = await _envelopeGroupLogic.Value.GetEnvelopeGroupsAsync();
            if (updatedEnvelopeGroup.Success)
            {
                var groupToRemove = EnvelopeGroups.FirstOrDefault(a => a.Id == envelopeGroup.Id);
                EnvelopeGroups.Remove(groupToRemove);
                EnvelopeGroups.Add(updatedEnvelopeGroup.Data.FirstOrDefault(g => g.Id == envelopeGroup.Id));
            }
        }

        public async Task ExecuteSaveSearchCommand()
        {
            var newEnvelopeGroup = new EnvelopeGroup
            {
                Description = SearchText
            };

            var result = await _envelopeGroupLogic.Value.SaveEnvelopeGroupAsync(newEnvelopeGroup);

            if (result.Success)
            {
                _needToSync = true;

                await ExecuteRefreshEnvelopeGroupCommand(result.Data);
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertAveUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
            }
        }

        public async Task ExecuteSaveCommand(EnvelopeGroup envelopeGroup)
        {
            var result = await _envelopeGroupLogic.Value.SaveEnvelopeGroupAsync(envelopeGroup);

            if (result.Success)
            {
                _needToSync = true;
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.Value.GetResourceString("AlertSaveUnsuccessful"), result.Message, _resourceContainer.Value.GetResourceString("AlertOk"));
            }
        }

        public async Task ExecuteAddCommand()
        {
            await _navigationService.NavigateAsync(PageName.EnvelopeGroupEditPage);

        }

        public async Task ExecuteEditCommand(EnvelopeGroup envelopeGroup)
        {
            var parameters = new NavigationParameters
            {
                { PageParameter.EnvelopeGroup, envelopeGroup }
            };
            await _navigationService.NavigateAsync(PageName.EnvelopeGroupEditPage, parameters);
        }
    }
}

