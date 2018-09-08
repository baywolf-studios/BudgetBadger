using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class TransactionViewCell : Grid
    {
        public static readonly BindableProperty ToggleCommandProperty =
            BindableProperty.Create("ToggleCommand", typeof(ICommand), typeof(TransactionViewCell), null, propertyChanged: OnToggleCommandPropertyChanged);

        public ICommand ToggleCommand
        {
            get { return (ICommand)GetValue(ToggleCommandProperty); }
            set { SetValue(ToggleCommandProperty, value); }
        }

        public static readonly BindableProperty SelectedCommandProperty =
            BindableProperty.Create("SelectedCommand", typeof(ICommand), typeof(TransactionViewCell), null, propertyChanged: OnSelectedCommandPropertyChanged);

        public ICommand SelectedCommand
        {
            get { return (ICommand)GetValue(SelectedCommandProperty); }
            set { SetValue(SelectedCommandProperty, value); }
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
                        rightSingleLabel.IsVisible = true;
                        rightPrimaryLabel.IsVisible = false;
                        rightSecondaryLabel.IsVisible = false;
                        primaryLabel.SetBinding(Label.TextProperty, "Payee.Description");
                        secondaryLabel.SetBinding(Label.TextProperty, "Account.Description");
                        rightSingleLabel.SetBinding(Label.TextProperty, "Amount", stringFormat: "{0:C}");
                        break;
                    case TransactionViewCellType.Account:
                        rightSingleLabel.IsVisible = true;
                        rightPrimaryLabel.IsVisible = false;
                        rightSecondaryLabel.IsVisible = false;
                        primaryLabel.SetBinding(Label.TextProperty, "Payee.Description");
                        secondaryLabel.SetBinding(Label.TextProperty, "Envelope.Description");
                        rightSingleLabel.SetBinding(Label.TextProperty, "Amount", stringFormat: "{0:C}");
                        break;
                    case TransactionViewCellType.Payee:
                        rightSingleLabel.IsVisible = true;
                        rightPrimaryLabel.IsVisible = false;
                        rightSecondaryLabel.IsVisible = false;
                        primaryLabel.SetBinding(Label.TextProperty, "Envelope.Description");
                        secondaryLabel.SetBinding(Label.TextProperty, "Account.Description");
                        rightSingleLabel.SetBinding(Label.TextProperty, "Amount", stringFormat: "{0:C}");
                        break;
                    case TransactionViewCellType.Full:
                        rightSingleLabel.IsVisible = false;
                        rightPrimaryLabel.IsVisible = true;
                        rightSecondaryLabel.IsVisible = true;
                        primaryLabel.SetBinding(Label.TextProperty, "Payee.Description");
                        secondaryLabel.SetBinding(Label.TextProperty, "Account.Description");
                        rightPrimaryLabel.SetBinding(Label.TextProperty, "Amount", stringFormat: "{0:C}");
                        rightSecondaryLabel.SetBinding(Label.TextProperty, "Envelope.Description");
                        break;
                }
            }            
        }

        public TransactionViewCell()
        {
            InitializeComponent();
        }

        static void OnToggleCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != oldValue)
            {
                (bindable as TransactionViewCell).ToggleGestureRecognizer.Command = newValue as ICommand;
            }
        }

        static void OnSelectedCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != oldValue)
            {
                (bindable as TransactionViewCell).SelectedGestureRecognizer.Command = newValue as ICommand;
            }
        }
    }
}
