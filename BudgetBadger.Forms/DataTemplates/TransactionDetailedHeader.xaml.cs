using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class TransactionDetailedHeader : Grid
    {
        TransactionViewCellType transactionViewCellType;
        public TransactionViewCellType TransactionViewCellType
        {
            get => transactionViewCellType;
            set
            {
                transactionViewCellType = value;
                switch (transactionViewCellType)
                {
                    case TransactionViewCellType.Envelope:
                        ColumnDefinitions.Remove(envelopeColumn);
                        if (!ColumnDefinitions.Contains(accountColumn))
                        {
                            ColumnDefinitions.Insert(1, accountColumn);
                        }
                        if (!ColumnDefinitions.Contains(payeeColumn))
                        {
                            ColumnDefinitions.Insert(2, accountColumn);
                        }
                        break;
                    case TransactionViewCellType.Account:
                        ColumnDefinitions.Remove(accountColumn);
                        if (!ColumnDefinitions.Contains(envelopeColumn))
                        {
                            ColumnDefinitions.Insert(1, envelopeColumn);
                        }
                        if (!ColumnDefinitions.Contains(payeeColumn))
                        {
                            ColumnDefinitions.Insert(2, accountColumn);
                        }
                        break;
                    case TransactionViewCellType.Payee:
                        ColumnDefinitions.Remove(payeeColumn);
                        if (!ColumnDefinitions.Contains(accountColumn))
                        {
                            ColumnDefinitions.Insert(1, accountColumn);
                        }
                        if (!ColumnDefinitions.Contains(envelopeColumn))
                        {
                            ColumnDefinitions.Insert(2, envelopeColumn);
                        }
                        break;
                    case TransactionViewCellType.Full:
                        ColumnDefinitions.Remove(payeeColumn);
                        if (!ColumnDefinitions.Contains(accountColumn))
                        {
                            ColumnDefinitions.Insert(1, accountColumn);
                        }
                        if (!ColumnDefinitions.Contains(envelopeColumn))
                        {
                            ColumnDefinitions.Insert(2, envelopeColumn);
                        }
                        if (!ColumnDefinitions.Contains(payeeColumn))
                        {
                            ColumnDefinitions.Insert(3, payeeColumn);
                        }
                        break;
                }
            }
        }

        public TransactionDetailedHeader()
        {
            InitializeComponent();
        }
    }
}
