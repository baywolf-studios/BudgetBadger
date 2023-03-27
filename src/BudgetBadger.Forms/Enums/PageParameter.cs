using System;
namespace BudgetBadger.Forms.Enums
{
    public static class PageParameter
    {
        public const string GoBack = nameof(GoBack);
        public const string Account = "account";
        public const string Budget = "budget";
        public const string Payee = "payee";
        public const string Transaction = "transaction";
        public const string SplitTransactionId = "splitTransactionId";
        public const string InitialSplitTransaction = "initialSplitTransaction";
        public const string SplitTransactionMode = "splitTransactionMode";
        public const string EnvelopeGroup = "envelopeGroup";
        public const string Envelope = "envelope";
        public const string BudgetSchedule = "budgetSchedule";
        public const string TransactionAmount = "transactionAmount";
		public const string TransactionServiceDate = "transactionServiceDate";
        public const string DeletedTransaction = "deletedTransaction";
        public const string ReportBeginDate = "reportBeginDate";
        public const string ReportEndDate = "reportEndDate";
        public const string ReconcileDate = "reconcileDate";
        public const string ReconcileAmount = "reconcileAmount";
        public const string ReconcileCompleted = "reconcileCompleted";
        public const string TransferEnvelopeSelection = "transferEnvelopeSelection";
        public const string LicenseName = "licenseName";
        public const string LicenseText = "licenseText";
        public const string PageName = "pageName";
    }
}
