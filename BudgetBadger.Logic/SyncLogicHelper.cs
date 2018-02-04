using System;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.DataAccess;
using BudgetBadger.Models;

namespace BudgetBadger.Logic
{
    public static class SyncLogicHelper
    {
        public async static Task SyncAccountTypes(IAccountDataAccess sourceAccountDataAccess, IAccountDataAccess targetAccountDataAccess)
        {
            var sourceAccountTypes = await sourceAccountDataAccess.ReadAccountTypesAsync();
            var targetAccountTypes = await targetAccountDataAccess.ReadAccountTypesAsync();

            var sourceAccountTypesDictionary = sourceAccountTypes.ToDictionary(a => a.Id, a2 => a2);
            var targetAccountTypesDictionary = targetAccountTypes.ToDictionary(a => a.Id, a2 => a2);

            var accountTypesToAdd = sourceAccountTypesDictionary.Keys.Except(targetAccountTypesDictionary.Keys);
            foreach (var accountTypeId in accountTypesToAdd)
            {
                var accountTypeToAdd = sourceAccountTypesDictionary[accountTypeId];
                await targetAccountDataAccess.CreateAccountTypeAsync(accountTypeToAdd);
            }

            var accountTypesToUpdate = sourceAccountTypesDictionary.Keys.Intersect(targetAccountTypesDictionary.Keys);
            foreach (var accountTypeId in accountTypesToUpdate)
            {
                var sourceAccountType = sourceAccountTypesDictionary[accountTypeId];
                var targetAccountType = targetAccountTypesDictionary[accountTypeId];

                if (sourceAccountType.ModifiedDateTime > targetAccountType.ModifiedDateTime)
                {
                    await targetAccountDataAccess.UpdateAccountTypeAsync(sourceAccountType);
                }
            }
        }

        public async static Task SyncAccounts(IAccountDataAccess sourceAccountDataAccess, IAccountDataAccess targetAccountDataAccess)
        {
            var sourceAccounts = await sourceAccountDataAccess.ReadAccountsAsync();
            var targetAccounts = await targetAccountDataAccess.ReadAccountsAsync();

            var sourceAccountsDictionary = sourceAccounts.ToDictionary(a => a.Id, a2 => a2);
            var targetAccountsDictionary = targetAccounts.ToDictionary(a => a.Id, a2 => a2);

            var accountsToAdd = sourceAccountsDictionary.Keys.Except(targetAccountsDictionary.Keys);
            foreach (var accountId in accountsToAdd)
            {
                var accountToAdd = sourceAccountsDictionary[accountId];
                await targetAccountDataAccess.CreateAccountAsync(accountToAdd);
            }

            var accountsToUpdate = sourceAccountsDictionary.Keys.Intersect(targetAccountsDictionary.Keys);
            foreach (var accountId in accountsToUpdate)
            {
                var sourceAccount = sourceAccountsDictionary[accountId];
                var targetAccount = targetAccountsDictionary[accountId];

                if (sourceAccount.ModifiedDateTime > targetAccount.ModifiedDateTime)
                {
                    await targetAccountDataAccess.UpdateAccountAsync(sourceAccount);
                }
            }
        }

        public async static Task SyncPayees(IPayeeDataAccess sourcePayeeDataAccess, IPayeeDataAccess targetPayeeDataAccess)
        {
            var sourcePayees = await sourcePayeeDataAccess.ReadPayeesAsync();
            var targetPayees = await targetPayeeDataAccess.ReadPayeesAsync();

            var sourcePayeesDictionary = sourcePayees.ToDictionary(a => a.Id, a2 => a2);
            var targetPayeesDictionary = targetPayees.ToDictionary(a => a.Id, a2 => a2);

            var payeesToAdd = sourcePayeesDictionary.Keys.Except(targetPayeesDictionary.Keys);
            foreach (var payeeId in payeesToAdd)
            {
                var payeeToAdd = sourcePayeesDictionary[payeeId];
                await targetPayeeDataAccess.CreatePayeeAsync(payeeToAdd);
            }

            var payeesToUpdate = sourcePayeesDictionary.Keys.Intersect(targetPayeesDictionary.Keys);
            foreach (var payeeId in payeesToUpdate)
            {
                var sourcePayee = sourcePayeesDictionary[payeeId];
                var targetPayee = targetPayeesDictionary[payeeId];

                if (sourcePayee.ModifiedDateTime > targetPayee.ModifiedDateTime)
                {
                    await targetPayeeDataAccess.UpdatePayeeAsync(sourcePayee);
                }
            }
        }

        public async static Task SyncEnvelopeGroups(IEnvelopeDataAccess sourceEnvelopeDataAccess, IEnvelopeDataAccess targetEnvelopeDataAccess)
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

        public async static Task SyncEnvelopes(IEnvelopeDataAccess sourceEnvelopeDataAccess, IEnvelopeDataAccess targetEnvelopeDataAccess)
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

        public async static Task SyncBudgetSchedules(IEnvelopeDataAccess sourceEnvelopeDataAccess, IEnvelopeDataAccess targetEnvelopeDataAccess)
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

        public async static Task SyncBudgets(IEnvelopeDataAccess sourceEnvelopeDataAccess, IEnvelopeDataAccess targetEnvelopeDataAccess)
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

        public async static Task SyncTransactions(ITransactionDataAccess sourceTransactionDataAccess, ITransactionDataAccess targetTransactionDataAccess)
        {
            var sourceTransactions = await sourceTransactionDataAccess.ReadTransactionsAsync();
            var targetTransactions = await targetTransactionDataAccess.ReadTransactionsAsync();

            var sourceTransactionsDictionary = sourceTransactions.ToDictionary(a => a.Id, a2 => a2);
            var targetTransactionsDictionary = targetTransactions.ToDictionary(a => a.Id, a2 => a2);

            var transactionsToAdd = sourceTransactionsDictionary.Keys.Except(targetTransactionsDictionary.Keys);
            foreach (var transactionId in transactionsToAdd)
            {
                var transactionToAdd = sourceTransactionsDictionary[transactionId];
                await targetTransactionDataAccess.CreateTransactionAsync(transactionToAdd);
            }

            var transactionsToUpdate = sourceTransactionsDictionary.Keys.Intersect(targetTransactionsDictionary.Keys);
            foreach (var transactionId in transactionsToUpdate)
            {
                var sourceTransaction = sourceTransactionsDictionary[transactionId];
                var targetTransaction = targetTransactionsDictionary[transactionId];

                if (sourceTransaction.ModifiedDateTime > targetTransaction.ModifiedDateTime)
                {
                    await targetTransactionDataAccess.UpdateTransactionAsync(sourceTransaction);
                }
            }
        }
    }
}
