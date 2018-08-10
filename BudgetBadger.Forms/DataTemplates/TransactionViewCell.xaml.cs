using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class TransactionViewCell : ViewCell
    {
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create("Command", typeof(ICommand), typeof(TransactionViewCell), null, propertyChanged: OnCommandPropertyChanged);

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        TransactionViewCellType transactionViewCellType;
        public TransactionViewCellType TransactionViewCellType
        {
            get => transactionViewCellType;
            set
            {
                transactionViewCellType = value;
                switch(transactionViewCellType)
                {
                    case TransactionViewCellType.Envelope:
                        primaryLabel.SetBinding(Label.TextProperty, "Payee.Description");
                        secondaryLabel.SetBinding(Label.TextProperty, "Account.Description");
                        break;
                    case TransactionViewCellType.Account:
                        primaryLabel.SetBinding(Label.TextProperty, "Payee.Description");
                        secondaryLabel.SetBinding(Label.TextProperty, "Envelope.Description");
                        break;
                    case TransactionViewCellType.Payee:
                        primaryLabel.SetBinding(Label.TextProperty, "Envelope.Description");
                        secondaryLabel.SetBinding(Label.TextProperty, "Account.Description");
                        break;
                }
            }            
        }

        public TransactionViewCell()
        {
            InitializeComponent();
        }

        static void OnCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != oldValue)
            {
                (bindable as TransactionViewCell).hiddenButton.Command = newValue as ICommand;
            }
        }
    }
}
