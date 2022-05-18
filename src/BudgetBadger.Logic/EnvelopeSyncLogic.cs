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
        readonly IDataAccess _localDataAccess;
        readonly IDataAccess _remoteDataAccess;

        public EnvelopeSyncLogic(IDataAccess localDataAccess,
                                 IDataAccess remoteDataAccess)
        {
            _localDataAccess = localDataAccess;
            _remoteDataAccess = remoteDataAccess;
        }

        public async Task<Result> PullAsync()
        {
            var result = new Result();

            try
            {
                await SyncEnvelopeGroups(_remoteDataAccess, _localDataAccess);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            try
            {
                await SyncEnvelopes(_remoteDataAccess, _localDataAccess);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            try
            {
                await SyncBudgetSchedules(_remoteDataAccess, _localDataAccess);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            try
            {
                await SyncBudgets(_remoteDataAccess, _localDataAccess);
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
                await SyncEnvelopeGroups(_localDataAccess, _remoteDataAccess);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            try
            {
                await SyncEnvelopes(_localDataAccess, _remoteDataAccess);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            try
            {
                await SyncBudgetSchedules(_localDataAccess, _remoteDataAccess);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }

            try
            {
                await SyncBudgets(_localDataAccess, _remoteDataAccess);
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

        async Task SyncEnvelopeGroups(IDataAccess sourceDataAccess, IDataAccess targetDataAccess)
        {
            await sourceDataAccess.Init();
            await targetDataAccess.Init();

            var sourceEnvelopeGroups = await sourceDataAccess.ReadEnvelopeGroupsAsync();
            var targetEnvelopeGroups = await targetDataAccess.ReadEnvelopeGroupsAsync();

            var sourceEnvelopeGroupsDictionary = sourceEnvelopeGroups.ToDictionary(a => a.Id, a2 => a2);
            var targetEnvelopeGroupsDictionary = targetEnvelopeGroups.ToDictionary(a => a.Id, a2 => a2);

            var envelopeGroupsToAdd = sourceEnvelopeGroupsDictionary.Keys.Except(targetEnvelopeGroupsDictionary.Keys);
            foreach (var envelopeGroupId in envelopeGroupsToAdd)
            {
                var envelopeGroupToAdd = sourceEnvelopeGroupsDictionary[envelopeGroupId];
                await targetDataAccess.CreateEnvelopeGroupAsync(envelopeGroupToAdd);
            }

            var envelopeGroupsToUpdate = sourceEnvelopeGroupsDictionary.Keys.Intersect(targetEnvelopeGroupsDictionary.Keys);
            foreach (var envelopeGroupId in envelopeGroupsToUpdate)
            {
                var sourceEnvelopeGroup = sourceEnvelopeGroupsDictionary[envelopeGroupId];
                var targetEnvelopeGroup = targetEnvelopeGroupsDictionary[envelopeGroupId];

                if (sourceEnvelopeGroup.ModifiedDateTime > targetEnvelopeGroup.ModifiedDateTime)
                {
                    await targetDataAccess.UpdateEnvelopeGroupAsync(sourceEnvelopeGroup);
                }
            }
        }

        async Task SyncEnvelopes(IDataAccess sourceDataAccess, IDataAccess targetDataAccess)
        {
            await sourceDataAccess.Init();
            await targetDataAccess.Init();

            var sourceEnvelopes = await sourceDataAccess.ReadEnvelopesAsync();
            var targetEnvelopes = await targetDataAccess.ReadEnvelopesAsync();

            var sourceEnvelopesDictionary = sourceEnvelopes.ToDictionary(a => a.Id, a2 => a2);
            var targetEnvelopesDictionary = targetEnvelopes.ToDictionary(a => a.Id, a2 => a2);

            var envelopesToAdd = sourceEnvelopesDictionary.Keys.Except(targetEnvelopesDictionary.Keys);
            foreach (var envelopeId in envelopesToAdd)
            {
                var envelopeToAdd = sourceEnvelopesDictionary[envelopeId];
                await targetDataAccess.CreateEnvelopeAsync(envelopeToAdd);
            }

            var envelopesToUpdate = sourceEnvelopesDictionary.Keys.Intersect(targetEnvelopesDictionary.Keys);
            foreach (var envelopeId in envelopesToUpdate)
            {
                var sourceEnvelope = sourceEnvelopesDictionary[envelopeId];
                var targetEnvelope = targetEnvelopesDictionary[envelopeId];

                if (sourceEnvelope.ModifiedDateTime > targetEnvelope.ModifiedDateTime)
                {
                    await targetDataAccess.UpdateEnvelopeAsync(sourceEnvelope);
                }
            }
        }

        async Task SyncBudgetSchedules(IDataAccess sourceDataAccess, IDataAccess targetDataAccess)
        {
            await sourceDataAccess.Init();
            await targetDataAccess.Init();

            var sourceBudgetSchedules = await sourceDataAccess.ReadBudgetSchedulesAsync();
            var targetBudgetSchedules = await targetDataAccess.ReadBudgetSchedulesAsync();

            var sourceBudgetSchedulesDictionary = sourceBudgetSchedules.ToDictionary(a => a.Id, a2 => a2);
            var targetBudgetSchedulesDictionary = targetBudgetSchedules.ToDictionary(a => a.Id, a2 => a2);

            var budgetSchedulesToAdd = sourceBudgetSchedulesDictionary.Keys.Except(targetBudgetSchedulesDictionary.Keys);
            foreach (var budgetScheduleId in budgetSchedulesToAdd)
            {
                var budgetScheduleToAdd = sourceBudgetSchedulesDictionary[budgetScheduleId];
                await targetDataAccess.CreateBudgetScheduleAsync(budgetScheduleToAdd);
            }

            var budgetSchedulesToUpdate = sourceBudgetSchedulesDictionary.Keys.Intersect(targetBudgetSchedulesDictionary.Keys);
            foreach (var budgetScheduleId in budgetSchedulesToUpdate)
            {
                var sourceBudgetSchedule = sourceBudgetSchedulesDictionary[budgetScheduleId];
                var targetBudgetSchedule = targetBudgetSchedulesDictionary[budgetScheduleId];

                if (sourceBudgetSchedule.ModifiedDateTime > targetBudgetSchedule.ModifiedDateTime)
                {
                    await targetDataAccess.UpdateBudgetScheduleAsync(sourceBudgetSchedule);
                }
            }
        }

        async Task SyncBudgets(IDataAccess sourceDataAccess, IDataAccess targetDataAccess)
        {
            await sourceDataAccess.Init();
            await targetDataAccess.Init();

            var sourceBudgets = await sourceDataAccess.ReadBudgetsAsync();
            var targetBudgets = await targetDataAccess.ReadBudgetsAsync();

            var sourceBudgetsDictionary = sourceBudgets.ToDictionary(a => a.Envelope.Id.ToString() + a.Schedule.Id.ToString(), a2 => a2);
            var targetBudgetsDictionary = targetBudgets.ToDictionary(a => a.Envelope.Id.ToString() + a.Schedule.Id.ToString(), a2 => a2);

            var budgetsToAdd = sourceBudgetsDictionary.Keys.Except(targetBudgetsDictionary.Keys);
            foreach (var budgetId in budgetsToAdd)
            {
                var budgetToAdd = sourceBudgetsDictionary[budgetId];
                await targetDataAccess.CreateBudgetAsync(budgetToAdd);
            }

            var budgetsToUpdate = sourceBudgetsDictionary.Keys.Intersect(targetBudgetsDictionary.Keys);
            foreach (var budgetId in budgetsToUpdate)
            {
                var sourceBudget = sourceBudgetsDictionary[budgetId];
                var targetBudget = targetBudgetsDictionary[budgetId];

                if (sourceBudget.ModifiedDateTime > targetBudget.ModifiedDateTime)
                {
                    await targetDataAccess.UpdateBudgetAsync(sourceBudget);
                }
            }
        }
    }
}
