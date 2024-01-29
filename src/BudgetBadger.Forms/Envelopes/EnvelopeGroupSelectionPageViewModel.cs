using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Logic;
using BudgetBadger.Forms.Enums;
using BudgetBadger.Forms.Events;
using BudgetBadger.Core.Models;
using Prism.Events;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;
using BudgetBadger.Forms.Extensions;

namespace BudgetBadger.Forms.Envelopes
{
    public class EnvelopeGroupSelectionPageViewModel : ObservableBase, INavigationAware, IInitializeAsync
    {
        readonly IResourceContainer _resourceContainer;
        readonly IEnvelopeLogic _envelopeLogic;
        readonly INavigationService _navigationService;
        readonly IPageDialogService _dialogService;
        readonly IEventAggregator _eventAggregator;

        public ICommand BackCommand { get => new Command(async () => await _navigationService.GoBackAsync()); }
        public ICommand SelectedCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand SaveSearchCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand ManageGroupsCommand { get => new Command(async () => await _navigationService.NavigateAsync(PageName.EnvelopeGroupsPage)); }
        public Predicate<object> Filter { get => (env) => _envelopeLogic.FilterEnvelopeGroup((EnvelopeGroupModel)env, SearchText); }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        EnvelopeGroupModel _selectedEnvelopeGroup;
        public EnvelopeGroupModel SelectedEnvelopeGroup
        {
            get => _selectedEnvelopeGroup;
            set => SetProperty(ref _selectedEnvelopeGroup, value);
        }

        ObservableList<EnvelopeGroupModel> _envelopeGroups;
        public ObservableList<EnvelopeGroupModel> EnvelopeGroups
        {
            get => _envelopeGroups;
            set => SetProperty(ref _envelopeGroups, value);
        }

        string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { SetProperty(ref _searchText, value); RaisePropertyChanged(nameof(HasSearchText)); }
        }

        public bool HasSearchText { get => !string.IsNullOrWhiteSpace(SearchText); }

        bool _noEnvelopeGroups;
        public bool NoEnvelopeGroups
        {
            get => _noEnvelopeGroups;
            set => SetProperty(ref _noEnvelopeGroups, value);
        }

        public EnvelopeGroupSelectionPageViewModel(IResourceContainer resourceContainer,
                                           INavigationService navigationService,
                                           IPageDialogService dialogService,
                                           IEnvelopeLogic envelopeLogic,
                                           IEventAggregator eventAggregator)
        {
            _resourceContainer = resourceContainer;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _envelopeLogic = envelopeLogic;
            _eventAggregator = eventAggregator;

            SelectedEnvelopeGroup = null;
            EnvelopeGroups = new ObservableList<EnvelopeGroupModel>();

            SelectedCommand = new Command<EnvelopeGroupModel>(async eg => await ExecuteSelectedCommand(eg));
            RefreshCommand = new Command(async () => await FullRefresh());
            SaveCommand = new Command(async () => await ExecuteSaveCommand());
            SaveSearchCommand = new Command(async () => await ExecuteSaveSearchCommand());
            AddCommand = new Command(async () => await ExecuteAddCommand());
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            SelectedEnvelopeGroup = null;
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            var envelopeGroup = parameters.GetValue<EnvelopeGroupModel>(PageParameter.EnvelopeGroup);
            if (envelopeGroup != null)
            {
                await _navigationService.GoBackAsync(parameters);
                return;
            }

            await FullRefresh();
        }

        public async Task InitializeAsync(INavigationParameters parameters)
        {
            var countResult = await _envelopeLogic.GetEnvelopeGroupsAsync();
            if (countResult.Success)
            {
                if (countResult.Data.Count <= 0)
                {
                    // add some 
                    var montlhyBills = new EnvelopeGroupModel { Id = Constants.MonthlyBillsEnvelopGroupId, Description = _resourceContainer.GetResourceString("EnvelopeGroupMonthlyBills") };
                    var result1 = await _envelopeLogic.SaveEnvelopeGroupAsync(montlhyBills);
                    _eventAggregator.GetEvent<EnvelopeGroupSavedEvent>().Publish(result1.Data);

                    var everydayExpenses = new EnvelopeGroupModel { Id = Constants.EverydayExpensesEnvelopeGroupId, Description = _resourceContainer.GetResourceString("EnvelopeGroupEverydayExpenses") };
                    var result2 = await _envelopeLogic.SaveEnvelopeGroupAsync(everydayExpenses);
                    _eventAggregator.GetEvent<EnvelopeGroupSavedEvent>().Publish(result2.Data);

                    var savingsGoals = new EnvelopeGroupModel { Id = Constants.SavingsGoalsEnvelopeGroupdId, Description = _resourceContainer.GetResourceString("EnvelopeGroupSavingsGoals") };
                    var result3 = await _envelopeLogic.SaveEnvelopeGroupAsync(savingsGoals);
                    _eventAggregator.GetEvent<EnvelopeGroupSavedEvent>().Publish(result3.Data);

                    // show message
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertSuggestedEnvelopeGroups"),
                        _resourceContainer.GetResourceString("AlertMessageSuggestedEnvelopeGroups"),
                        _resourceContainer.GetResourceString("AlertOk"));
                }
            }
        }

        public async Task FullRefresh()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                var result = await _envelopeLogic.GetEnvelopeGroupsForSelectionAsync();

                if (result.Success)
                {
                    EnvelopeGroups.ReplaceRange(result.Data);
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertRefreshUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                }

                NoEnvelopeGroups = (EnvelopeGroups?.Count ?? 0) == 0;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteSelectedCommand(EnvelopeGroupModel envelopeGroup)
        {
            if (envelopeGroup == null)
            {
                return;
            }

            var parameters = new NavigationParameters
            {
                { PageParameter.EnvelopeGroup, envelopeGroup }
            };

            await _navigationService.GoBackAsync(parameters);
        }

        public async Task ExecuteSaveCommand()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                var newEnvelopeGroup = new EnvelopeGroupModel
                {
                    Description = SearchText
                };

                var result = await _envelopeLogic.SaveEnvelopeGroupAsync(newEnvelopeGroup);

                if (result.Success)
                {
                    _eventAggregator.GetEvent<EnvelopeGroupSavedEvent>().Publish(result.Data);

                    await _navigationService.GoBackAsync();
                }
                else
                {
                    await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertSaveUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteSaveSearchCommand()
        {
            var newEnvelopeGroup = new EnvelopeGroupModel
            {
                Description = SearchText
            };

            var result = await _envelopeLogic.SaveEnvelopeGroupAsync(newEnvelopeGroup);

            if (result.Success)
            {
                _eventAggregator.GetEvent<EnvelopeGroupSavedEvent>().Publish(result.Data);
                var parameters = new NavigationParameters
                {
                    { PageParameter.EnvelopeGroup, result.Data }
                };

                await _navigationService.GoBackAsync(parameters);
            }
            else
            {
                await _dialogService.DisplayAlertAsync(_resourceContainer.GetResourceString("AlertSaveUnsuccessful"), result.Message, _resourceContainer.GetResourceString("AlertOk"));
            }
        }

        public async Task ExecuteAddCommand()
        {
            await _navigationService.NavigateAsync(PageName.EnvelopeGroupEditPage);
        }
    }
}
