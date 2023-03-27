using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Logic;
using BudgetBadger.Core.Models;
using BudgetBadger.Logic;
using BudgetBadger.Logic.Converters;
using BudgetBadger.Logic.Models;

namespace BudgetBadger.Forms.Extensions
{
    public static class IPayeeLogicExtensions
    {
        public static bool FilterPayee(this IPayeeLogic _, PayeeModel payee, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return true;
            }
            if (payee != null)
            {
                return payee.Description.ToLower().Contains(searchText.ToLower());
            }
            else
            {
                return false;
            }
        }

        public static bool FilterPayee(this IPayeeLogic _, PayeeModel payee, FilterType filterType)
        {
            switch (filterType)
            {
                case FilterType.Editable:
                case FilterType.Reportable:
                    return payee.IsActive && !payee.IsStartingBalance && !payee.IsAccount;
                case FilterType.Selectable:
                    return payee.IsActive && !payee.IsStartingBalance;
                case FilterType.Hidden:
                    return payee.IsHidden && !payee.IsDeleted && !payee.IsStartingBalance && !payee.IsAccount;
                case FilterType.None:
                default:
                    return true;
            }
        }

        public async static Task<Result<PayeeModel>> SavePayeeAsync(this IPayeeLogic payeeLogic, PayeeModel payee)
        {
            if (payee.IsNew)
            {
                var result = await payeeLogic.CreatePayeeAsync(payee.Description, payee.Notes, payee.HiddenDateTime != null);
                if (result)
                {
                    var payeeResult = await payeeLogic.ReadPayeeAsync(result.Data);
                    if (payeeResult)
                    {
                        return Result.Ok(PayeeModelConverter.Convert(payeeResult.Data));
                    }
                    else
                    {
                        return Result.Fail<PayeeModel>(payeeResult.Message);
                    }
                }
                else
                {
                    return Result.Fail<PayeeModel>(result.Message);
                }
            }
            else
            {
                var result = await payeeLogic.UpdatePayeeAsync(new PayeeId(payee.Id), payee.Description, payee.Notes, payee.HiddenDateTime != null);
                if (result)
                {
                    var payeeResult = await payeeLogic.ReadPayeeAsync(new PayeeId(payee.Id));
                    if (payeeResult)
                    {
                        return Result.Ok(PayeeModelConverter.Convert(payeeResult.Data));
                    }
                    else
                    {
                        return Result.Fail<PayeeModel>(payeeResult.Message);
                    }
                }
                else
                {
                    return Result.Fail<PayeeModel>(result.Message);
                }
            }
        }

        public static async Task<Result<PayeeModel>> GetPayeeAsync(this IPayeeLogic payeeLogic, Guid id)
        {
            var result = await payeeLogic.ReadPayeeAsync(new PayeeId(id));
            if (result)
            {
                return Result.Ok(PayeeModelConverter.Convert(result.Data));
            }
            else
            {
                return Result.Fail<PayeeModel>(result.Message);
            }
        }

        public async static Task<Result<IReadOnlyList<PayeeModel>>> GetPayeesAsync(this IPayeeLogic payeeLogic)
        {
            var result = await payeeLogic.SearchPayeesAsync(hidden: false, isAccount: false, isStartingBalance: false);
            if (result)
            {
                var payees = result.Items.Select(PayeeModelConverter.Convert).ToList();
                var hiddenResult = await payeeLogic.SearchPayeesAsync(hidden: true, isAccount: false, isStartingBalance: false);
                if (hiddenResult && hiddenResult.TotalItemsCount > 0)
                {
                    payees.Add(new PayeeModel()
                    {
                        Id = Constants.GenericHiddenPayeeId,
                        Description = AppResources.Hidden,
                        Group = AppResources.Hidden,
                        CreatedDateTime = DateTime.Now,
                        ModifiedDateTime = DateTime.Now,
                        HiddenDateTime = DateTime.Now
                    });
                }
                return Result.Ok<IReadOnlyList<PayeeModel>>(payees);
            }
            else
            {
                return Result.Fail<IReadOnlyList<PayeeModel>>(result.Message);
            }
        }

        public static async Task<Result<IReadOnlyList<PayeeModel>>> GetPayeesForSelectionAsync(this IPayeeLogic payeeLogic)
        {
            var result = await payeeLogic.SearchPayeesAsync(hidden: false, isAccount: true, isStartingBalance: false);
            if (result)
            {
                return Result.Ok<IReadOnlyList<PayeeModel>>(result.Items.Select(PayeeModelConverter.Convert).ToList());
            }
            else
            {
                return Result.Fail<IReadOnlyList<PayeeModel>>(result.Message);
            }
        }

        public static async Task<Result<IReadOnlyList<PayeeModel>>> GetPayeesForReportAsync(this IPayeeLogic payeeLogic)
        {
            var result = await payeeLogic.SearchPayeesAsync(hidden: false, isAccount: false, isStartingBalance: false);
            if (result)
            {
                return Result.Ok<IReadOnlyList<PayeeModel>>(result.Items.Select(PayeeModelConverter.Convert).ToList());
            }
            else
            {
                return Result.Fail<IReadOnlyList<PayeeModel>>(result.Message);
            }
        }

        public static async Task<Result<IReadOnlyList<PayeeModel>>> GetHiddenPayeesAsync(this IPayeeLogic payeeLogic)
        {
            var result = await payeeLogic.SearchPayeesAsync(hidden: true, isAccount: false, isStartingBalance: false);
            if (result)
            {
                return Result.Ok<IReadOnlyList<PayeeModel>>(result.Items.Select(PayeeModelConverter.Convert).ToList());
            }
            else
            {
                return Result.Fail<IReadOnlyList<PayeeModel>>(result.Message);
            }
        }

        public static async Task<Result<PayeeModel>> SoftDeletePayeeAsync(this IPayeeLogic payeeLogic, Guid id)
        {
            var payeeResult = await payeeLogic.ReadPayeeAsync(new PayeeId(id));
            if (payeeResult)
            {
                var deleteResult = await payeeLogic.DeletePayeeAsync(new PayeeId(id));
                if (deleteResult)
                {
                    var payeeModel = PayeeModelConverter.Convert(payeeResult.Data);
                    payeeModel.DeletedDateTime = DateTime.Now;
                    payeeModel.ModifiedDateTime = DateTime.Now;
                    return Result.Ok(payeeModel);
                }
                else
                {
                    return Result.Fail<PayeeModel>(deleteResult.Message);
                }
            }
            else
            {
                return Result.Fail<PayeeModel>(payeeResult.Message);
            }
        }

        public async static Task<Result<PayeeModel>> HidePayeeAsync(this IPayeeLogic payeeLogic, Guid id)
        {
            var payeeResult = await payeeLogic.ReadPayeeAsync(new PayeeId(id));
            if (payeeResult)
            {
                var payee = payeeResult.Data with { Hidden = true };
                var updateResult = await payeeLogic.UpdatePayeeAsync(payee.Id, payee.Description, payee.Notes, payee.Hidden);
                if (updateResult)
                {
                    var payeeModel = PayeeModelConverter.Convert(payee);
                    return Result.Ok(payeeModel);
                }
                else
                {
                    return Result.Fail<PayeeModel>(updateResult.Message);
                }
            }
            else
            {
                return Result.Fail<PayeeModel>(payeeResult.Message);
            }
        }

        public async static Task<Result<PayeeModel>> UnhidePayeeAsync(this IPayeeLogic payeeLogic, Guid id)
        {
            var payeeResult = await payeeLogic.ReadPayeeAsync(new PayeeId(id));
            if (payeeResult)
            {
                var payee = payeeResult.Data with { Hidden = false };
                var updateResult = await payeeLogic.UpdatePayeeAsync(payee.Id, payee.Description, payee.Notes, payee.Hidden);
                if (updateResult)
                {
                    var payeeModel = PayeeModelConverter.Convert(payee);
                    return Result.Ok(payeeModel);
                }
                else
                {
                    return Result.Fail<PayeeModel>(updateResult.Message);
                }
            }
            else
            {
                return Result.Fail<PayeeModel>(payeeResult.Message);
            }
        }
    }
}

