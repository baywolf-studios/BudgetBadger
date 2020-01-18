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
                        accountColumn.Width = new GridLength(1, GridUnitType.Star);
                        envelopeColumn.Width = new GridLength(0);
                        payeeColumn.Width = new GridLength(1, GridUnitType.Star);
                        break;
                    case TransactionViewCellType.Account:
                        accountColumn.Width = new GridLength(0);
                        envelopeColumn.Width = new GridLength(1, GridUnitType.Star);
                        payeeColumn.Width = new GridLength(1, GridUnitType.Star);
                        break;
                    case TransactionViewCellType.Payee:
                        accountColumn.Width = new GridLength(1, GridUnitType.Star);
                        envelopeColumn.Width = new GridLength(1, GridUnitType.Star);
                        payeeColumn.Width = new GridLength(0);
                        break;
                    case TransactionViewCellType.Full:
                        accountColumn.Width = new GridLength(1, GridUnitType.Star);
                        envelopeColumn.Width = new GridLength(1, GridUnitType.Star);
                        payeeColumn.Width = new GridLength(1, GridUnitType.Star);
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
