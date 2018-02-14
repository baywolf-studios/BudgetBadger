using System;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Core.Logic;
using BudgetBadger.Models;

namespace BudgetBadger.Logic
{
    public class EnvelopeSyncLogic : IEnvelopeSyncLogic
    {
        readonly IEnvelopeDataAccess _localEnvelopeDataAccess;
        readonly IEnvelopeDataAccess _remoteEnvelopeDataAccess;

        public EnvelopeSyncLogic(IEnvelopeDataAccess localEnvelopeDataAccess,
                                 IEnvelopeDataAccess remoteEnvelopeDataAccess)
        {
            _localEnvelopeDataAccess = localEnvelopeDataAccess;
            _remoteEnvelopeDataAccess = remoteEnvelopeDataAccess;
        }

        public async Task<Result> PullAsync()
        {
            var result = new Result();

            try
            {
                await SyncEnvelopeGroups(_remoteEnvelopeDataAccess, _localEnvelopeDataAccess);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            try
            {
                await SyncEnvelopes(_remoteEnvelopeDataAccess, _localEnvelopeDataAccess);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            try
            {
                await SyncBudgetSchedules(_remoteEnvelopeDataAccess, _localEnvelopeDataAccess);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            try
            {
                await SyncBudgets(_remoteEnvelopeDataAccess, _localEnvelopeDataAccess);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            result.Success = true;
            return result;
        }

        public async Task<Result> PushAsync()
        {
            var result = new Result();

            try
            {
                await SyncEnvelopeGroups(_localEnvelopeDataAccess, _remoteEnvelopeDataAccess);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            try
            {
                await SyncEnvelopes(_localEnvelopeDataAccess, _remoteEnvelopeDataAccess);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            try
            {
                await SyncBudgetSchedules(_localEnvelopeDataAccess, _remoteEnvelopeDataAccess);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            try
            {
                await SyncBudgets(_localEnvelopeDataAccess, _remoteEnvelopeDataAccess);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            result.Success = true;
            return result;
        }

        public async Task<Result> SyncAsync()
        {
            var result = new Result();

            result = await PullAsync();

            if (result.Success)
            {
                result = await PushAsync();
            }

            return result;
        }

        async Task SyncEnvelopeGroups(IEnvelopeDataAccess sourceEnvelopeDataAccess, IEnvelopeDataAccess targetEnvelopeDataAccess)
        {
            var sourceEnvelopeGroups = await sourceEnvelopeDataAccess.ReadEnvelopeGroupsAsync();
            var targetEnvelopeGroups = await targetEnvelopeDataAccess.ReadEnvelopeGroupsAsync();

            var sourceEnvelopeGroupsDictionary = sourceEnvelopeGroups.ToDictionary(a => a.Id, a2 => a2);
            var targetEnvelopeGroupsDictionary = targetEnvelopeGroups.ToDictionary(a => a.Id, a2 => a2);

            var envelopeGroupsToAdd = sourceEnvelopeGroupsDictionary.Keys.Except(targetEnvelopeGroupsDictionary.Keys);
            foreach (var envelopeGroupId in envelopeGroupsToAdd)
            {
                var envelopeGroupToAdd = sourceEnvelopeGroupsDictionary[envelopeGroupId];
                await targetEnvelopeDataAccess.CreateEnvelopeGroupAsync(envelopeGroupToAdd);
            }

            var envelopeGroupsToUpdate = sourceEnvelopeGroupsDictionary.Keys.Intersect(targetEnvelopeGroupsDictionary.Keys);
            foreach (var envelopeGroupId in envelopeGroupsToUpdate)
            {
                var sourceEnvelopeGroup = sourceEnvelopeGroupsDictionary[envelopeGroupId];
                var targetEnvelopeGroup = targetEnvelopeGroupsDictionary[envelopeGroupId];

                if (sourceEnvelopeGroup.ModifiedDateTime > targetEnvelopeGroup.ModifiedDateTime)
                {
                    await targetEnvelopeDataAccess.UpdateEnvelopeGroupAsync(sourceEnvelopeGroup);
                }
            }
        }

        async Task SyncEnvelopes(IEnvelopeDataAccess sourceEnvelopeDataAccess, IEnvelopeDataAccess targetEnvelopeDataAccess)
        {
            var sourceEnvelopes = await sourceEnvelopeDataAccess.ReadEnvelopesAsync();
            var targetEnvelopes = await targetEnvelopeDataAccess.ReadEnvelopesAsync();

            var sourceEnvelopesDictionary = sourceEnvelopes.ToDictionary(a => a.Id, a2 => a2);
            var targetEnvelopesDictionary = targetEnvelopes.ToDictionary(a => a.Id, a2 => a2);

            var envelopesToAdd = sourceEnvelopesDictionary.Keys.Except(targetEnvelopesDictionary.Keys);
            foreach (var envelopeId in envelopesToAdd)
            {
                var envelopeToAdd = sourceEnvelopesDictionary[envelopeId];
                await targetEnvelopeDataAccess.CreateEnvelopeAsync(envelopeToAdd);
            }

            var envelopesToUpdate = sourceEnvelopesDictionary.Keys.Intersect(targetEnvelopesDictionary.Keys);
            foreach (var envelopeId in envelopesToUpdate)
            {
                var sourceEnvelope = sourceEnvelopesDictionary[envelopeId];
                var targetEnvelope = targetEnvelopesDictionary[envelopeId];

                if (sourceEnvelope.ModifiedDateTime > targetEnvelope.ModifiedDateTime)
                {
                    await targetEnvelopeDataAccess.UpdateEnvelopeAsync(sourceEnvelope);
                }
            }
        }

        async Task SyncBudgetSchedules(IEnvelopeDataAccess sourceEnvelopeDataAccess, IEnvelopeDataAccess targetEnvelopeDataAccess)
        {
            var sourceBudgetSchedules = await sourceEnvelopeDataAccess.ReadBudgetSchedulesAsync();
            var targetBudgetSchedules = await targetEnvelopeDataAccess.ReadBudgetSchedulesAsync();

            var sourceBudgetSchedulesDictionary = sourceBudgetSchedules.ToDictionary(a => a.Id, a2 => a2);
            var targetBudgetSchedulesDictionary = targetBudgetSchedules.ToDictionary(a => a.Id, a2 => a2);

            var budgetSchedulesToAdd = sourceBudgetSchedulesDictionary.Keys.Except(targetBudgetSchedulesDictionary.Keys);
            foreach (var budgetScheduleId in budgetSchedulesToAdd)
            {
                var budgetScheduleToAdd = sourceBudgetSchedulesDictionary[budgetScheduleId];
                await targetEnvelopeDataAccess.CreateBudgetScheduleAsync(budgetScheduleToAdd);
            }

            var budgetSchedulesToUpdate = sourceBudgetSchedulesDictionary.Keys.Intersect(targetBudgetSchedulesDictionary.Keys);
            foreach (var budgetScheduleId in budgetSchedulesToUpdate)
            {
                var sourceBudgetSchedule = sourceBudgetSchedulesDictionary[budgetScheduleId];
                var targetBudgetSchedule = targetBudgetSchedulesDictionary[budgetScheduleId];

                if (sourceBudgetSchedule.ModifiedDateTime > targetBudgetSchedule.ModifiedDateTime)
                {
                    await targetEnvelopeDataAccess.UpdateBudgetScheduleAsync(sourceBudgetSchedule);
                }
            }
        }

        async Task SyncBudgets(IEnvelopeDataAccess sourceEnvelopeDataAccess, IEnvelopeDataAccess targetEnvelopeDataAccess)
        {
            var sourceBudgets = await sourceEnvelopeDataAccess.ReadBudgetsAsync();
            var targetBudgets = await targetEnvelopeDataAccess.ReadBudgetsAsync();

            var sourceBudgetsDictionary = sourceBudgets.ToDictionary(a => a.Id, a2 => a2);
            var targetBudgetsDictionary = targetBudgets.ToDictionary(a => a.Id, a2 => a2);

            var budgetsToAdd = sourceBudgetsDictionary.Keys.Except(targetBudgetsDictionary.Keys);
            foreach (var budgetId in budgetsToAdd)
            {
                var budgetToAdd = sourceBudgetsDictionary[budgetId];
                await targetEnvelopeDataAccess.CreateBudgetAsync(budgetToAdd);
            }

            var budgetsToUpdate = sourceBudgetsDictionary.Keys.Intersect(targetBudgetsDictionary.Keys);
            foreach (var budgetId in budgetsToUpdate)
            {
                var sourceBudget = sourceBudgetsDictionary[budgetId];
                var targetBudget = targetBudgetsDictionary[budgetId];

                if (sourceBudget.ModifiedDateTime > targetBudget.ModifiedDateTime)
                {
                    await targetEnvelopeDataAccess.UpdateBudgetAsync(sourceBudget);
                }
            }
        }
    }
}
