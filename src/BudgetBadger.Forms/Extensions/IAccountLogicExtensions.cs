using System;
using BudgetBadger.Core.Models;
using BudgetBadger.Logic.Converters;
using BudgetBadger.Logic.Models;
using System.Threading.Tasks;
using BudgetBadger.Core.Logic;
using BudgetBadger.Logic;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Core.Localization;

namespace BudgetBadger.Forms.Extensions
{
    public static class IAccountLogicExtensions
    {
        public static bool FilterAccount(this IAccountLogic _, AccountModel account, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return true;
            }

            if (account != null)
            {
                return account.Description.ToLower().Contains(searchText.ToLower());
            }
            else
            {
                return false;
            }
        }

        public static bool FilterAccount(this IAccountLogic _, AccountModel account, FilterType filterType)
        {
            switch (filterType)
            {
                case FilterType.Editable:
                case FilterType.Reportable:
                case FilterType.Selectable:
                    return account.IsActive;
                case FilterType.Hidden:
                    return account.IsHidden && !account.IsDeleted;
                case FilterType.None:
                default:
                    return true;
            }
        }

        public async static Task<Result<AccountModel>> SaveAccountAsync(this IAccountLogic accountLogic, AccountModel account)
        {
            if (account.IsNew)
            {
                if (account.Balance == null)
                {
                    return Result.Fail<AccountModel>(AppResources.AccountValidBalanceError);
                }

                var result = await accountLogic.CreateAccountAsync(
                    account.Description,
                    account.Notes,
                    account.OnBudget ? AccountType.Budget : AccountType.Reporting,
                    account.Balance ?? 0,
                    account.HiddenDateTime != null);

                if (result)
                {
                    var accountResult = await accountLogic.ReadAccountAsync(result.Data);
                    if (accountResult)
                    {
                        return Result.Ok(AccountModelConverter.Convert(accountResult.Data));
                    }
                    else
                    {
                        return Result.Fail<AccountModel>(accountResult.Message);
                    }
                }
                else
                {
                    return Result.Fail<AccountModel>(result.Message);
                }
            }
            else
            {
                var result = await accountLogic.UpdateAccountAsync(new AccountId(account.Id), account.Description, account.Notes, account.HiddenDateTime != null);
                if (result)
                {
                    var accountResult = await accountLogic.ReadAccountAsync(new AccountId(account.Id));
                    if (accountResult)
                    {
                        return Result.Ok(AccountModelConverter.Convert(accountResult.Data));
                    }
                    else
                    {
                        return Result.Fail<AccountModel>(accountResult.Message);
                    }
                }
                else
                {
                    return Result.Fail<AccountModel>(result.Message);
                }
            }
        }

        public static async Task<Result<IReadOnlyList<AccountModel>>> GetAccountsAsync(this IAccountLogic accountLogic)
        {
            var result = await accountLogic.SearchAccountsAsync(hidden: false);
            if (result)
            {
                var accounts = result.Items.Select(AccountModelConverter.Convert).ToList();
                var hiddenResult = await accountLogic.SearchAccountsAsync(hidden: true);
                if (hiddenResult && hiddenResult.TotalItemsCount > 0)
                {
                    accounts.Add(new AccountModel
                    {
                        Id = Constants.GenericHiddenAccountId,
                        Description = AppResources.Hidden,
                        Group = AppResources.Hidden,
                        Pending = hiddenResult.Items.Sum(a => a.Pending),
                        Posted = hiddenResult.Items.Sum(a => a.Posted),
                        Balance = hiddenResult.Items.Sum(a => a.Balance),
                        Payment = hiddenResult.Items.Sum(a => a.Payment),
                        CreatedDateTime = DateTime.Now,
                        ModifiedDateTime = DateTime.Now,
                        HiddenDateTime = DateTime.Now
                    });
                }
                return Result.Ok<IReadOnlyList<AccountModel>>(accounts);
            }
            else
            {
                return Result.Fail<IReadOnlyList<AccountModel>>(result.Message);
            }
        }

        public static async Task<Result<IReadOnlyList<AccountModel>>> GetAccountsForSelectionAsync(this IAccountLogic accountLogic)
        {
            var result = await accountLogic.SearchAccountsAsync(hidden: false);
            if (result)
            {
                return Result.Ok<IReadOnlyList<AccountModel>>(result.Items.Select(AccountModelConverter.Convert).ToList());
            }
            else
            {
                return Result.Fail<IReadOnlyList<AccountModel>>(result.Message);
            }
        }

        public static async Task<Result<IReadOnlyList<AccountModel>>> GetHiddenAccountsAsync(this IAccountLogic accountLogic)
        {
            var result = await accountLogic.SearchAccountsAsync(hidden: true);
            if (result)
            {
                return Result.Ok<IReadOnlyList<AccountModel>>(result.Items.Select(AccountModelConverter.Convert).ToList());
            }
            else
            {
                return Result.Fail<IReadOnlyList<AccountModel>>(result.Message);
            }
        }

        public static async Task<Result<AccountModel>> GetAccountAsync(this IAccountLogic accountLogic, Guid id)
        {
            var result = await accountLogic.ReadAccountAsync(new AccountId(id));
            if (result)
            {
                return Result.Ok(AccountModelConverter.Convert(result.Data));
            }
            else
            {
                return Result.Fail<AccountModel>(result.Message);
            }
        }

        public async static Task<Result<AccountModel>> HideAccountAsync(this IAccountLogic accountLogic, Guid id)
        {
            var accountResult = await accountLogic.ReadAccountAsync(new AccountId(id));
            if (accountResult)
            {
                var account = accountResult.Data with { Hidden = true };
                var updateResult = await accountLogic.UpdateAccountAsync(new AccountId(account.Id), account.Description, account.Notes, account.Hidden);
                if (updateResult)
                {
                    var accountModel = AccountModelConverter.Convert(account);
                    return Result.Ok(accountModel);
                }
                else
                {
                    return Result.Fail<AccountModel>(updateResult.Message);
                }
            }
            else
            {
                return Result.Fail<AccountModel>(accountResult.Message);
            }
        }

        public async static Task<Result<AccountModel>> UnhideAccountAsync(this IAccountLogic accountLogic, Guid id)
        {
            var accountResult = await accountLogic.ReadAccountAsync(new AccountId(id));
            if (accountResult)
            {
                var account = accountResult.Data with { Hidden = false };
                var updateResult = await accountLogic.UpdateAccountAsync(new AccountId(account.Id), account.Description, account.Notes, account.Hidden);
                if (updateResult)
                {
                    var accountModel = AccountModelConverter.Convert(account);
                    return Result.Ok(accountModel);
                }
                else
                {
                    return Result.Fail<AccountModel>(updateResult.Message);
                }
            }
            else
            {
                return Result.Fail<AccountModel>(accountResult.Message);
            }
        }

        public static async Task<Result<AccountModel>> SoftDeleteAccountAsync(this IAccountLogic accountLogic, Guid id)
        {
            var accountResult = await accountLogic.ReadAccountAsync(new AccountId(id));
            if (accountResult)
            {
                var deleteResult = await accountLogic.DeleteAccountAsync(new AccountId(id));
                if (deleteResult)
                {
                    var accountModel = AccountModelConverter.Convert(accountResult.Data);
                    accountModel.DeletedDateTime = DateTime.Now;
                    accountModel.ModifiedDateTime = DateTime.Now;
                    return Result.Ok(accountModel);
                }
                else
                {
                    return Result.Fail<AccountModel>(deleteResult.Message);
                }
            }
            else
            {
                return Result.Fail<AccountModel>(accountResult.Message);
            }
        }

        public static async Task<Result> ReconcileAccount(this IAccountLogic accountLogic, Guid accountId, DateTime dateTime, decimal amount)
        {
            var result = await accountLogic.ReconcileAccountAsync(new AccountId(accountId), dateTime, amount);

            if (result)
            {
                return Result.Ok();
            }
            else
            {
                return Result.Fail(result.Message);
            }
        }
    }
}

