using System;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;

namespace BudgetBadger.Core.Logic
{
    public class MergeLogic : IMergeLogic
    {
        public async Task MergeAccountsAsync(IDataAccess sourceDataAccess, IDataAccess targetDataAccess)
        {
            var sourceAccounts = await sourceDataAccess.ReadAccountsAsync();
            var targetAccounts = await targetDataAccess.ReadAccountsAsync();

            if (sourceAccounts.Any(a => a.Id == Guid.Empty)) throw new Exception("Source Account missing Id");

            var sourceAccountsDictionary = sourceAccounts.ToDictionary(a => a.Id, a2 => a2);
            var targetAccountsDictionary = targetAccounts.ToDictionary(a => a.Id, a2 => a2);

            var accountsToAdd = sourceAccountsDictionary.Keys.Except(targetAccountsDictionary.Keys);
            foreach (var accountId in accountsToAdd)
            {
                var accountToAdd = sourceAccountsDictionary[accountId];
                await targetDataAccess.CreateAccountAsync(accountToAdd);
            }

            var accountsToUpdate = sourceAccountsDictionary.Keys.Intersect(targetAccountsDictionary.Keys);
            foreach (var accountId in accountsToUpdate)
            {
                var sourceAccount = sourceAccountsDictionary[accountId];
                var targetAccount = targetAccountsDictionary[accountId];

                if (sourceAccount.ModifiedDateTime > targetAccount.ModifiedDateTime)
                    await targetDataAccess.UpdateAccountAsync(sourceAccount);
            }
        }
        
        public async Task MergePayeesAsync(IDataAccess sourceDataAccess, IDataAccess targetDataAccess)
        {
            var sourcePayees = await sourceDataAccess.ReadPayeesAsync();
            var targetPayees = await targetDataAccess.ReadPayeesAsync();

            if (sourcePayees.Any(a => a.Id == Guid.Empty)) throw new Exception("Source Payee missing Id");

            var sourcePayeesDictionary = sourcePayees.ToDictionary(a => a.Id, a2 => a2);
            var targetPayeesDictionary = targetPayees.ToDictionary(a => a.Id, a2 => a2);

            var payeesToAdd = sourcePayeesDictionary.Keys.Except(targetPayeesDictionary.Keys);
            foreach (var payeeId in payeesToAdd)
            {
                var payeeToAdd = sourcePayeesDictionary[payeeId];
                await targetDataAccess.CreatePayeeAsync(payeeToAdd);
            }

            var payeesToUpdate = sourcePayeesDictionary.Keys.Intersect(targetPayeesDictionary.Keys);
            foreach (var payeeId in payeesToUpdate)
            {
                var sourcePayee = sourcePayeesDictionary[payeeId];
                var targetPayee = targetPayeesDictionary[payeeId];

                if (sourcePayee.ModifiedDateTime > targetPayee.ModifiedDateTime)
                    await targetDataAccess.UpdatePayeeAsync(sourcePayee);
            }
        }

        public async Task MergeEnvelopeGroupsAsync(IDataAccess sourceDataAccess, IDataAccess targetDataAccess)
        {
            var sourceEnvelopeGroups = await sourceDataAccess.ReadEnvelopeGroupsAsync();
            var targetEnvelopeGroups = await targetDataAccess.ReadEnvelopeGroupsAsync();

            if (sourceEnvelopeGroups.Any(a => a.Id == Guid.Empty)) throw new Exception("Source EnvelopeGroup missing Id");
            
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

        public async Task MergeEnvelopesAsync(IDataAccess sourceDataAccess, IDataAccess targetDataAccess)
        {
            var sourceEnvelopes = await sourceDataAccess.ReadEnvelopesAsync();
            var targetEnvelopes = await targetDataAccess.ReadEnvelopesAsync();

            if (sourceEnvelopes.Any(a => a.Id == Guid.Empty)) throw new Exception("Source Envelope missing Id");
            if (sourceEnvelopes.Any(a => a.Group == null || a.Group.Id == Guid.Empty)) throw new Exception("Source Envelope missing EnvelopeGroup");

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

        public async Task MergeBudgetSchedulesAsync(IDataAccess sourceDataAccess, IDataAccess targetDataAccess)
        {
            var sourceBudgetSchedules = await sourceDataAccess.ReadBudgetSchedulesAsync();
            var targetBudgetSchedules = await targetDataAccess.ReadBudgetSchedulesAsync();

            if (sourceBudgetSchedules.Any(a => a.Id == Guid.Empty)) throw new Exception("Source BudgetSchedule missing Id");

            
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

        public async Task MergeBudgetsAsync(IDataAccess sourceDataAccess, IDataAccess targetDataAccess)
        {
            var sourceBudgets = await sourceDataAccess.ReadBudgetsAsync();
            var targetBudgets = await targetDataAccess.ReadBudgetsAsync();

            if (sourceBudgets.Any(a => a.Id == Guid.Empty)) throw new Exception("Source Budget missing Id");
            if (sourceBudgets.Any(a => a.Schedule == null || a.Schedule.Id == Guid.Empty)) throw new Exception("Source Budget missing BudgetSchedule");
            if (sourceBudgets.Any(a => a.Envelope == null || a.Envelope.Id == Guid.Empty)) throw new Exception("Source Budget missing Envelope");

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

        public async Task MergeTransactionsAsync(IDataAccess sourceDataAccess, IDataAccess targetDataAccess)
        {
            var sourceTransactions = await sourceDataAccess.ReadTransactionsAsync();
            var targetTransactions = await targetDataAccess.ReadTransactionsAsync();

            if (sourceTransactions.Any(a => a.Id == Guid.Empty)) throw new Exception("Source Budget missing Id");
            if (sourceTransactions.Any(a => a.Account == null || a.Account.Id == Guid.Empty)) throw new Exception("Source Transaction missing Account");
            if (sourceTransactions.Any(a => a.Envelope == null || a.Envelope.Id == Guid.Empty)) throw new Exception("Source Transaction missing Envelope");
            if (sourceTransactions.Any(a => a.Payee == null || a.Payee.Id == Guid.Empty)) throw new Exception("Source Transaction missing Payee");
            
            var sourceTransactionsDictionary = sourceTransactions.ToDictionary(a => a.Id, a2 => a2);
            var targetTransactionsDictionary = targetTransactions.ToDictionary(a => a.Id, a2 => a2);

            var transactionsToAdd = sourceTransactionsDictionary.Keys.Except(targetTransactionsDictionary.Keys);
            foreach (var transactionId in transactionsToAdd)
            {
                var transactionToAdd = sourceTransactionsDictionary[transactionId];
                await targetDataAccess.CreateTransactionAsync(transactionToAdd);
            }

            var transactionsToUpdate = sourceTransactionsDictionary.Keys.Intersect(targetTransactionsDictionary.Keys);
            foreach (var transactionId in transactionsToUpdate)
            {
                var sourceTransaction = sourceTransactionsDictionary[transactionId];
                var targetTransaction = targetTransactionsDictionary[transactionId];

                if (sourceTransaction.ModifiedDateTime > targetTransaction.ModifiedDateTime)
                {
                    await targetDataAccess.UpdateTransactionAsync(sourceTransaction);
                }
            }
        }

        public async Task MergeAllAsync(IDataAccess sourceDataAccess, IDataAccess targetDataAccess)
        {
            await MergeAccountsAsync(sourceDataAccess, targetDataAccess);
            await MergePayeesAsync(sourceDataAccess, targetDataAccess);
            await MergeEnvelopeGroupsAsync(sourceDataAccess, targetDataAccess);
            await MergeEnvelopesAsync(sourceDataAccess, targetDataAccess);
            await MergeBudgetSchedulesAsync(sourceDataAccess, targetDataAccess);
            await MergeBudgetsAsync(sourceDataAccess, targetDataAccess);
            await MergeTransactionsAsync(sourceDataAccess, targetDataAccess);
        }
    }
}