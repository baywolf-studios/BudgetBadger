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
                        Children.Remove(envelopeControl);
                        SetColumn(accountControl, 1);
                        SetColumn(payeeControl, 2);
                        SetColumnSpan(divider, 7);
                        SetColumn(outflowControl, 3);
                        SetColumn(inflowControl, 4);
                        ColumnDefinitions.Remove(envelopeColumn);
                        break;
                    case TransactionViewCellType.Account:
                        Children.Remove(accountControl);
                        SetColumn(envelopeControl, 1);
                        SetColumn(payeeControl, 2);
                        SetColumnSpan(divider, 7);
                        SetColumn(outflowControl, 3);
                        SetColumn(inflowControl, 4);
                        ColumnDefinitions.Remove(accountColumn);
                        break;
                    case TransactionViewCellType.Payee:
                        Children.Remove(payeeControl);
                        SetColumn(accountControl, 1);
                        SetColumn(envelopeControl, 2);
                        SetColumnSpan(divider, 7);
                        SetColumn(outflowControl, 3);
                        SetColumn(inflowControl, 4);
                        ColumnDefinitions.Remove(payeeColumn);
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
