using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Models.Interfaces;

namespace BudgetBadger.Models
{
    public class SplitTransaction : Transaction, IValidatable, IDeepCopy<SplitTransaction>
    {
        IList<Transaction> transactions;
        public IList<Transaction> Transactions
        {
            get => transactions;
            set => SetProperty(ref transactions, value);
        }

        public SplitTransaction()
        {
            Transactions = new List<Transaction>();
        }

        SplitTransaction IDeepCopy<SplitTransaction>.DeepCopy()
        {
            SplitTransaction transaction = (SplitTransaction)DeepCopy();
            transaction.Transactions = new List<Transaction>(Transactions.Select(t => t.DeepCopy()));
            return transaction;
        }
    }
}
