using System;
namespace BudgetBadger.Forms.Enums
{
    public static class PageParameter
    {
        public static readonly string GoBack = nameof(GoBack);
        public static readonly string Account = "account";
        public static readonly string Budget = "budget";
        public static readonly string Payee = "payee";
        public static readonly string Transaction = "transaction";
        public static readonly string SplitTransactionId = "splitTransactionId";
        public static readonly string InitialSplitTransaction = "initialSplitTransaction";
        public static readonly string SplitTransactionMode = "splitTransactionMode";
        public static readonly string EnvelopeGroup = "envelopeGroup";
        public static readonly string Envelope = "envelope";
        public static readonly string BudgetSchedule = "budgetSchedule";
        public static readonly string TransactionAmount = "transactionAmount";
		public static readonly string TransactionServiceDate = "transactionServiceDate";
        public static readonly string DeletedTransaction = "deletedTransaction";
        public static readonly string ReportBeginDate = "reportBeginDate";
        public static readonly string ReportEndDate = "reportEndDate";
        public static readonly string ReconcileDate = "reconcileDate";
        public static readonly string ReconcileAmount = "reconcileAmount";
        public static readonly string ReconcileCompleted = "reconcileCompleted";
        public static readonly string TransferEnvelopeSelection = "transferEnvelopeSelection";
        public static readonly string PageName = "pageName";
    }
}
