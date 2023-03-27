using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.Converters;
using BudgetBadger.Core.Localization;
using BudgetBadger.Core.Logic;
using BudgetBadger.Core.Models;
using BudgetBadger.Logic;
using BudgetBadger.Logic.Converters;
using BudgetBadger.Logic.Models;

namespace BudgetBadger.Forms.Extensions
{
    public static class IEnvelopeLogicExtensions
    {
        public static bool FilterEnvelopeGroup(this IEnvelopeLogic _, EnvelopeGroupModel envelopeGroup, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return true;
            }
            if (envelopeGroup != null)
            {
                return envelopeGroup.Description.ToLower().Contains(searchText.ToLower());
            }
            else
            {
                return false;
            }
        }

        public static bool FilterEnvelopeGroup(this IEnvelopeLogic _, EnvelopeGroupModel envelopeGroup, FilterType filterType)
        {
            switch (filterType)
            {
                case FilterType.Editable:
                case FilterType.Reportable:
                case FilterType.Selectable:
                    return envelopeGroup.IsActive && !envelopeGroup.IsSystem && !envelopeGroup.IsIncome && !envelopeGroup.IsDebt;
                case FilterType.Hidden:
                    return envelopeGroup.IsHidden && !envelopeGroup.IsDeleted && !envelopeGroup.IsSystem && !envelopeGroup.IsIncome && !envelopeGroup.IsDebt;
                case FilterType.None:
                default:
                    return true;
            }
        }

        public static async Task<Result<IReadOnlyList<EnvelopeGroupModel>>> GetEnvelopeGroupsAsync(this IEnvelopeLogic envelopeLogic)
        {
            var result = await envelopeLogic.SearchEnvelopeGroupsAsync(hidden: false, isSystem: false, isIncome: false, isDebt: false);
            if (result)
            {
                var envelopeGroups = result.Items.Select(EnvelopeGroupModelConverter.Convert).ToList();
                var hiddenResult = await envelopeLogic.SearchEnvelopeGroupsAsync(hidden: true, isSystem: false, isIncome: false, isDebt: false);
                if (hiddenResult && hiddenResult.TotalItemsCount > 0)
                {
                    envelopeGroups.Add(new EnvelopeGroupModel
                    {
                        Id = Constants.GenericHiddenEnvelopeGroupId,
                        Description = AppResources.Hidden,
                        CreatedDateTime = DateTime.Now,
                        ModifiedDateTime = DateTime.Now,
                        HiddenDateTime = DateTime.Now
                    });
                }

                return Result.Ok<IReadOnlyList<EnvelopeGroupModel>>(envelopeGroups);
            }
            else
            {
                return Result.Fail<IReadOnlyList<EnvelopeGroupModel>>(result.Message);
            }
        }

        public static async Task<Result<IReadOnlyList<EnvelopeGroupModel>>> GetEnvelopeGroupsForSelectionAsync(this IEnvelopeLogic envelopeLogic)
        {
            var result = await envelopeLogic.SearchEnvelopeGroupsAsync(hidden: false, isSystem: false, isIncome: false, isDebt: false);
            if (result)
            {
                return Result.Ok<IReadOnlyList<EnvelopeGroupModel>>(result.Items.Select(EnvelopeGroupModelConverter.Convert).ToList());
            }
            else
            {
                return Result.Fail<IReadOnlyList<EnvelopeGroupModel>>(result.Message);
            }
        }

        public static async Task<Result<IReadOnlyList<EnvelopeGroupModel>>> GetHiddenEnvelopeGroupsAsync(this IEnvelopeLogic envelopeLogic)
        {
            var result = await envelopeLogic.SearchEnvelopeGroupsAsync(hidden: true, isSystem: false, isIncome: false, isDebt: false);
            if (result)
            {
                return Result.Ok<IReadOnlyList<EnvelopeGroupModel>>(result.Items.Select(EnvelopeGroupModelConverter.Convert).ToList());
            }
            else
            {
                return Result.Fail<IReadOnlyList<EnvelopeGroupModel>>(result.Message);
            }
        }

        public static async Task<Result<EnvelopeGroupModel>> SaveEnvelopeGroupAsync(this IEnvelopeLogic envelopeLogic, EnvelopeGroupModel envelopeGroup)
        {
            if (envelopeGroup.IsNew)
            {
                var result = await envelopeLogic.CreateEnvelopeGroupAsync(envelopeGroup.Description, envelopeGroup.Notes, envelopeGroup.HiddenDateTime != null);
                if (result)
                {
                    var envelopeGroupResult = await envelopeLogic.ReadEnvelopeGroupAsync(result.Data);
                    if (envelopeGroupResult)
                    {
                        return Result.Ok(EnvelopeGroupModelConverter.Convert(envelopeGroupResult.Data));
                    }
                    else
                    {
                        return Result.Fail<EnvelopeGroupModel>(envelopeGroupResult.Message);
                    }
                }
                else
                {
                    return Result.Fail<EnvelopeGroupModel>(result.Message);
                }
            }
            else
            {
                var result = await envelopeLogic.UpdateEnvelopeGroupAsync(new EnvelopeGroupId(envelopeGroup.Id), envelopeGroup.Description, envelopeGroup.Notes, envelopeGroup.HiddenDateTime != null);
                if (result)
                {
                    var envelopeGroupResult = await envelopeLogic.ReadEnvelopeGroupAsync(new EnvelopeGroupId(envelopeGroup.Id));
                    if (envelopeGroupResult)
                    {
                        return Result.Ok(EnvelopeGroupModelConverter.Convert(envelopeGroupResult.Data));
                    }
                    else
                    {
                        return Result.Fail<EnvelopeGroupModel>(envelopeGroupResult.Message);
                    }
                }
                else
                {
                    return Result.Fail<EnvelopeGroupModel>(result.Message);
                }
            }
        }

        public static async Task<Result<EnvelopeGroupModel>> GetEnvelopeGroupAsync(this IEnvelopeLogic envelopeLogic, Guid id)
        {
            var result = await envelopeLogic.ReadEnvelopeGroupAsync(new EnvelopeGroupId(id));
            if (result)
            {
                return Result.Ok(EnvelopeGroupModelConverter.Convert(result.Data));
            }
            else
            {
                return Result.Fail<EnvelopeGroupModel>(result.Message);
            }
        }

        public static async Task<Result<EnvelopeGroupModel>> SoftDeleteEnvelopeGroupAsync(this IEnvelopeLogic envelopeLogic, Guid id)
        {
            var envelopeGroupResult = await envelopeLogic.ReadEnvelopeGroupAsync(new EnvelopeGroupId(id));
            if (envelopeGroupResult)
            {
                var deleteResult = await envelopeLogic.DeleteEnvelopeGroupAsync(new EnvelopeGroupId(id));
                if (deleteResult)
                {
                    var envelopeGroupModel = EnvelopeGroupModelConverter.Convert(envelopeGroupResult.Data);
                    envelopeGroupModel.DeletedDateTime = DateTime.Now;
                    envelopeGroupModel.ModifiedDateTime = DateTime.Now;
                    return Result.Ok(envelopeGroupModel);
                }
                else
                {
                    return Result.Fail<EnvelopeGroupModel>(deleteResult.Message);
                }
            }
            else
            {
                return Result.Fail<EnvelopeGroupModel>(envelopeGroupResult.Message);
            }
        }

        public static async Task<Result<EnvelopeGroupModel>> HideEnvelopeGroupAsync(this IEnvelopeLogic envelopeLogic, Guid id)
        {
            var envelopeGroupResult = await envelopeLogic.ReadEnvelopeGroupAsync(new EnvelopeGroupId(id));
            if (envelopeGroupResult)
            {
                var envelopeGroup = envelopeGroupResult.Data with { Hidden = true };
                var updateResult = await envelopeLogic.UpdateEnvelopeGroupAsync(envelopeGroup.Id, envelopeGroup.Description, envelopeGroup.Notes, envelopeGroup.Hidden);
                if (updateResult)
                {
                    var envelopeGroupModel = EnvelopeGroupModelConverter.Convert(envelopeGroup);
                    return Result.Ok(envelopeGroupModel);
                }
                else
                {
                    return Result.Fail<EnvelopeGroupModel>(updateResult.Message);
                }
            }
            else
            {
                return Result.Fail<EnvelopeGroupModel>(envelopeGroupResult.Message);
            }
        }

        public static async Task<Result<EnvelopeGroupModel>> UnhideEnvelopeGroupAsync(this IEnvelopeLogic envelopeLogic, Guid id)
        {
            var envelopeGroupResult = await envelopeLogic.ReadEnvelopeGroupAsync(new EnvelopeGroupId(id));
            if (envelopeGroupResult)
            {
                var envelopeGroup = envelopeGroupResult.Data with { Hidden = false };
                var updateResult = await envelopeLogic.UpdateEnvelopeGroupAsync(envelopeGroup.Id, envelopeGroup.Description, envelopeGroup.Notes, envelopeGroup.Hidden);
                if (updateResult)
                {
                    var envelopeGroupModel = EnvelopeGroupModelConverter.Convert(envelopeGroup);
                    return Result.Ok(envelopeGroupModel);
                }
                else
                {
                    return Result.Fail<EnvelopeGroupModel>(updateResult.Message);
                }
            }
            else
            {
                return Result.Fail<EnvelopeGroupModel>(envelopeGroupResult.Message);
            }
        }

    }
}

