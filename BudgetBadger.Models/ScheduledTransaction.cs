using System;
namespace BudgetBadger.Models
{
    public class ScheduledTransaction : Transaction
    {
        string frequency;
        public string Frequency
        {
            get => frequency;
            set => SetProperty(ref frequency, value);
        }

        DateTime endDate;
        public DateTime EndDate
        {
            get => endDate.Date;
            set => SetProperty(ref endDate, value.Date);
        }

        public ScheduledTransaction()
        {
            Id = Guid.Empty;
            ServiceDate = DateTime.Now;
            Account = new Account();
            Payee = new Payee();
            Envelope = new Envelope();
        }

        public new ScheduledTransaction DeepCopy()
        {
            ScheduledTransaction scheduledTransaction = (ScheduledTransaction)base.DeepCopy();
            scheduledTransaction.EndDate = this.EndDate;
            scheduledTransaction.Frequency = this.Frequency == null ? null : String.Copy(this.Frequency);
            return scheduledTransaction;
        }

        public bool Equals(ScheduledTransaction p)
        {
            // If parameter is null, return false.
            if (p is null)
            {
                return false;
            }

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, p))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != p.GetType())
            {
                return false;
            }

            // Return true if the fields match.
            // Note that the base class is not invoked because it is
            // System.Object, which defines Equals as reference equality.
            return Id == p.Id
                && CreatedDateTime == p.CreatedDateTime
                && ModifiedDateTime == p.ModifiedDateTime
                && DeletedDateTime == p.DeletedDateTime
                && Notes == p.Notes
                && Amount == p.Amount
                && Posted == p.Posted
                && ReconciledDateTime == p.ReconciledDateTime
                && ServiceDate == p.ServiceDate
                && SplitId == p.SplitId
                && Account.Equals(p.Account)
                && Payee.Equals(p.Payee)
                && Envelope.Equals(p.Envelope)
                && Frequency == p.Frequency;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as ScheduledTransaction);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public int CompareTo(ScheduledTransaction transaction)
        {
            if (transaction is null)
            {
                return 1;
            }

            if (ServiceDate.Date.Equals(transaction.ServiceDate.Date))
            {
                return -1 * Nullable.Compare(CreatedDateTime, transaction.CreatedDateTime);
            }

            return -1 * ServiceDate.Date.CompareTo(transaction.ServiceDate.Date);
        }

        public new int CompareTo(object obj)
        {
            return CompareTo(obj as ScheduledTransaction);
        }
    }
}
