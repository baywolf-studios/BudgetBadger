﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BudgetBadger.Language {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class AppResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal AppResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("BudgetBadger.Language.AppResources", typeof(AppResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot delete account with balance.
        /// </summary>
        public static string AccountDeleteBalanceError {
            get {
                return ResourceManager.GetString("AccountDeleteBalanceError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot delete account with future transactions.
        /// </summary>
        public static string AccountDeleteFutureTransactionsError {
            get {
                return ResourceManager.GetString("AccountDeleteFutureTransactionsError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot delete an inactive account.
        /// </summary>
        public static string AccountDeleteInactiveError {
            get {
                return ResourceManager.GetString("AccountDeleteInactiveError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot delete account with pending transactions.
        /// </summary>
        public static string AccountDeletePendingTransactionsError {
            get {
                return ResourceManager.GetString("AccountDeletePendingTransactionsError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The reconciled amounts do not match.
        /// </summary>
        public static string AccountReconcileAmountsDoNotMatchError {
            get {
                return ResourceManager.GetString("AccountReconcileAmountsDoNotMatchError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Account is not deleted.
        /// </summary>
        public static string AccountUndoDeleteNotDeletedError {
            get {
                return ResourceManager.GetString("AccountUndoDeleteNotDeletedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Account balance is required.
        /// </summary>
        public static string AccountValidBalanceError {
            get {
                return ResourceManager.GetString("AccountValidBalanceError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Account name is required.
        /// </summary>
        public static string AccountValidDescriptionError {
            get {
                return ResourceManager.GetString("AccountValidDescriptionError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Buffer.
        /// </summary>
        public static string BufferEnvelope {
            get {
                return ResourceManager.GetString("BufferEnvelope", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Debt.
        /// </summary>
        public static string DebtEnvelopeGroup {
            get {
                return ResourceManager.GetString("DebtEnvelopeGroup", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot delete debt envelopes.
        /// </summary>
        public static string EnvelopeDeleteDebtError {
            get {
                return ResourceManager.GetString("EnvelopeDeleteDebtError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Envelope has future budget amounts to it.
        /// </summary>
        public static string EnvelopeDeleteFutureBudgetsError {
            get {
                return ResourceManager.GetString("EnvelopeDeleteFutureBudgetsError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Envelope has future transactions.
        /// </summary>
        public static string EnvelopeDeleteFutureTransactionsError {
            get {
                return ResourceManager.GetString("EnvelopeDeleteFutureTransactionsError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot delete income envelopes.
        /// </summary>
        public static string EnvelopeDeleteIncomeError {
            get {
                return ResourceManager.GetString("EnvelopeDeleteIncomeError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Envelope still has a remaining balance.
        /// </summary>
        public static string EnvelopeDeleteRemainingBalanceError {
            get {
                return ResourceManager.GetString("EnvelopeDeleteRemainingBalanceError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Envelope group name is required.
        /// </summary>
        public static string EnvelopeGroupValidDescriptionError {
            get {
                return ResourceManager.GetString("EnvelopeGroupValidDescriptionError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Budget amount is required.
        /// </summary>
        public static string EnvelopeValidAmountError {
            get {
                return ResourceManager.GetString("EnvelopeValidAmountError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Envelope name is required.
        /// </summary>
        public static string EnvelopeValidDescriptionError {
            get {
                return ResourceManager.GetString("EnvelopeValidDescriptionError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A valid Envelope is required.
        /// </summary>
        public static string EnvelopeValidEnvelopeError {
            get {
                return ResourceManager.GetString("EnvelopeValidEnvelopeError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Envelope is required.
        /// </summary>
        public static string EnvelopeValidEnvelopeExistError {
            get {
                return ResourceManager.GetString("EnvelopeValidEnvelopeExistError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Envelope group is required.
        /// </summary>
        public static string EnvelopeValidGroupError {
            get {
                return ResourceManager.GetString("EnvelopeValidGroupError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A valid envelope group is required.
        /// </summary>
        public static string EnvelopeValidGroupValidError {
            get {
                return ResourceManager.GetString("EnvelopeValidGroupValidError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ignore Overspend must be set on debt envelopes.
        /// </summary>
        public static string EnvelopeValidOverspendDebtError {
            get {
                return ResourceManager.GetString("EnvelopeValidOverspendDebtError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot set Ignore Overspend Always when Ignore Overspend is not set.
        /// </summary>
        public static string EnvelopeValidOverspendError {
            get {
                return ResourceManager.GetString("EnvelopeValidOverspendError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A valid Schedule is required.
        /// </summary>
        public static string EnvelopeValidScheduleError {
            get {
                return ResourceManager.GetString("EnvelopeValidScheduleError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Schedule is required.
        /// </summary>
        public static string EnvelopeValidScheduleExistError {
            get {
                return ResourceManager.GetString("EnvelopeValidScheduleExistError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Debt.
        /// </summary>
        public static string GenericDebtEnvelope {
            get {
                return ResourceManager.GetString("GenericDebtEnvelope", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Not Needed.
        /// </summary>
        public static string IgnoredEnvelope {
            get {
                return ResourceManager.GetString("IgnoredEnvelope", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Income.
        /// </summary>
        public static string IncomeEnvelope {
            get {
                return ResourceManager.GetString("IncomeEnvelope", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Income.
        /// </summary>
        public static string IncomeEnvelopeGroup {
            get {
                return ResourceManager.GetString("IncomeEnvelopeGroup", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot delete an account payee.
        /// </summary>
        public static string PayeeDeleteAccountError {
            get {
                return ResourceManager.GetString("PayeeDeleteAccountError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot delete a payee with future transactions on it.
        /// </summary>
        public static string PayeeDeleteFutureTransactionsError {
            get {
                return ResourceManager.GetString("PayeeDeleteFutureTransactionsError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot delete an inactive payee.
        /// </summary>
        public static string PayeeDeleteInactiveError {
            get {
                return ResourceManager.GetString("PayeeDeleteInactiveError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot delete the starting balance payee.
        /// </summary>
        public static string PayeeDeleteStartingBalanceError {
            get {
                return ResourceManager.GetString("PayeeDeleteStartingBalanceError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Payee name is required.
        /// </summary>
        public static string PayeeValidDescriptionError {
            get {
                return ResourceManager.GetString("PayeeValidDescriptionError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Avg. Past 3 Months Activity.
        /// </summary>
        public static string QuickBudgetAvgPast3MonthsActivity {
            get {
                return ResourceManager.GetString("QuickBudgetAvgPast3MonthsActivity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Avg. Past 3 Months Budgeted.
        /// </summary>
        public static string QuickBudgetAvgPast3MonthsBudgeted {
            get {
                return ResourceManager.GetString("QuickBudgetAvgPast3MonthsBudgeted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Avg. Past Year Activity.
        /// </summary>
        public static string QuickBudgetAvgPastYearActivity {
            get {
                return ResourceManager.GetString("QuickBudgetAvgPastYearActivity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Avg. Past Year Budgeted.
        /// </summary>
        public static string QuickBudgetAvgPastYearBudgeted {
            get {
                return ResourceManager.GetString("QuickBudgetAvgPastYearBudgeted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Balance.
        /// </summary>
        public static string QuickBudgetBalance {
            get {
                return ResourceManager.GetString("QuickBudgetBalance", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Last Month Activity.
        /// </summary>
        public static string QuickBudgetLastMonthActivity {
            get {
                return ResourceManager.GetString("QuickBudgetLastMonthActivity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Last Month Budgeted.
        /// </summary>
        public static string QuickBudgetLastMonthBudgeted {
            get {
                return ResourceManager.GetString("QuickBudgetLastMonthBudgeted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Budget schedule description is required.
        /// </summary>
        public static string ScheduleValidDescriptionError {
            get {
                return ResourceManager.GetString("ScheduleValidDescriptionError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Split.
        /// </summary>
        public static string Split {
            get {
                return ResourceManager.GetString("Split", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No split transactions to save.
        /// </summary>
        public static string SplitTransactionValidTransactionsError {
            get {
                return ResourceManager.GetString("SplitTransactionValidTransactionsError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Starting Balance.
        /// </summary>
        public static string StartingBalancePayee {
            get {
                return ResourceManager.GetString("StartingBalancePayee", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to System.
        /// </summary>
        public static string SystemEnvelopeGroup {
            get {
                return ResourceManager.GetString("SystemEnvelopeGroup", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot delete transaction with deleted account.
        /// </summary>
        public static string TransactionDeleteAccountDeletedError {
            get {
                return ResourceManager.GetString("TransactionDeleteAccountDeletedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot delete transaction with deleted envelope.
        /// </summary>
        public static string TransactionDeleteEnvelopeDeletedError {
            get {
                return ResourceManager.GetString("TransactionDeleteEnvelopeDeletedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot delete transfer transaction with deleted payee.
        /// </summary>
        public static string TransactionDeleteTransferDeletedPayeeError {
            get {
                return ResourceManager.GetString("TransactionDeleteTransferDeletedPayeeError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot edit the Account on a transaction with a deleted Account.
        /// </summary>
        public static string TransactionValidAccountDeletedAccountError {
            get {
                return ResourceManager.GetString("TransactionValidAccountDeletedAccountError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot use a deleted Account.
        /// </summary>
        public static string TransactionValidAccountDeletedError {
            get {
                return ResourceManager.GetString("TransactionValidAccountDeletedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Account is required.
        /// </summary>
        public static string TransactionValidAccountError {
            get {
                return ResourceManager.GetString("TransactionValidAccountError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Account does not exist.
        /// </summary>
        public static string TransactionValidAccountExistError {
            get {
                return ResourceManager.GetString("TransactionValidAccountExistError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot edit the Amount on a transaction with a deleted Account.
        /// </summary>
        public static string TransactionValidAmountDeletedAccountError {
            get {
                return ResourceManager.GetString("TransactionValidAmountDeletedAccountError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot edit the Amount on a transaction with a deleted Envelope.
        /// </summary>
        public static string TransactionValidAmountDeletedEnvelopeError {
            get {
                return ResourceManager.GetString("TransactionValidAmountDeletedEnvelopeError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot edit the Amount on a transaction with a deleted Payee.
        /// </summary>
        public static string TransactionValidAmountDeletedPayeeError {
            get {
                return ResourceManager.GetString("TransactionValidAmountDeletedPayeeError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Amount is required.
        /// </summary>
        public static string TransactionValidAmountError {
            get {
                return ResourceManager.GetString("TransactionValidAmountError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot use a deleted Envelope.
        /// </summary>
        public static string TransactionValidEnvelopeDeletedError {
            get {
                return ResourceManager.GetString("TransactionValidEnvelopeDeletedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Envelope is required.
        /// </summary>
        public static string TransactionValidEnvelopeError {
            get {
                return ResourceManager.GetString("TransactionValidEnvelopeError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Envelope does not exist.
        /// </summary>
        public static string TransactionValidEnvelopeExistError {
            get {
                return ResourceManager.GetString("TransactionValidEnvelopeExistError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot edit the Envelope on a transaction with a deleted Envelope.
        /// </summary>
        public static string TransactionValidPayeeDeletedEnvelopeError {
            get {
                return ResourceManager.GetString("TransactionValidPayeeDeletedEnvelopeError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot use a deleted Payee.
        /// </summary>
        public static string TransactionValidPayeeDeletedError {
            get {
                return ResourceManager.GetString("TransactionValidPayeeDeletedError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot edit the Payee on a transaction with a deleted Payee.
        /// </summary>
        public static string TransactionValidPayeeDeletedPayeeError {
            get {
                return ResourceManager.GetString("TransactionValidPayeeDeletedPayeeError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Payee is required.
        /// </summary>
        public static string TransactionValidPayeeError {
            get {
                return ResourceManager.GetString("TransactionValidPayeeError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Payee does not exist.
        /// </summary>
        public static string TransactionValidPayeeExistError {
            get {
                return ResourceManager.GetString("TransactionValidPayeeExistError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot edit the Service Date on a transaction with a deleted Account.
        /// </summary>
        public static string TransactionValidServiceDateDeletedAccountError {
            get {
                return ResourceManager.GetString("TransactionValidServiceDateDeletedAccountError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot edit the Service Date on a transaction with a deleted Envelope.
        /// </summary>
        public static string TransactionValidServiceDateDeletedEnvelopeError {
            get {
                return ResourceManager.GetString("TransactionValidServiceDateDeletedEnvelopeError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot edit the Service Date on a transaction with a deleted Payee.
        /// </summary>
        public static string TransactionValidServiceDateDeletedPayeeError {
            get {
                return ResourceManager.GetString("TransactionValidServiceDateDeletedPayeeError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Transfer does not contain a valid From Envelope.
        /// </summary>
        public static string TransferValidFromerror {
            get {
                return ResourceManager.GetString("TransferValidFromerror", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Transfer does not contain a valid To Envelope.
        /// </summary>
        public static string TransferValidToError {
            get {
                return ResourceManager.GetString("TransferValidToError", resourceCulture);
            }
        }
    }
}
