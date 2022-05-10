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
            BindableProperty.Create(nameof(ToggleCommand), typeof(ICommand), typeof(TransactionViewCell), null, propertyChanged: OnToggleCommandPropertyChanged);

        public ICommand ToggleCommand
        {
            get { return (ICommand)GetValue(ToggleCommandProperty); }
            set { SetValue(ToggleCommandProperty, value); }
        }

        public static readonly BindableProperty SelectedCommandProperty =
            BindableProperty.Create(nameof(SelectedCommand), typeof(ICommand), typeof(TransactionViewCell), null);

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
                switch (transactionViewCellType)
                {
                    case TransactionViewCellType.Envelope:
                        rightSingleLabel.IsVisible = true;
                        rightPrimaryLabel.IsVisible = false;
                        rightSecondaryLabel.IsVisible = false;
                        leftPrimaryLabel.SetBinding(Label.TextProperty, "Payee.Description");
                        leftSecondaryLabel.SetBinding(Label.TextProperty, "Account.Description");
                        rightSingleLabel.SetBinding(Label.TextProperty, "Amount", converter: (IValueConverter)Application.Current.Resources["CurrencyConverter"]);
                        break;
                    case TransactionViewCellType.Account:
                        rightSingleLabel.IsVisible = true;
                        rightPrimaryLabel.IsVisible = false;
                        rightSecondaryLabel.IsVisible = false;
                        leftPrimaryLabel.SetBinding(Label.TextProperty, "Payee.Description");
                        leftSecondaryLabel.SetBinding(Label.TextProperty, "Envelope.Description");
                        rightSingleLabel.SetBinding(Label.TextProperty, "Amount", converter: (IValueConverter)Application.Current.Resources["CurrencyConverter"]);
                        break;
                    case TransactionViewCellType.Payee:
                        rightSingleLabel.IsVisible = true;
                        rightPrimaryLabel.IsVisible = false;
                        rightSecondaryLabel.IsVisible = false;
                        leftPrimaryLabel.SetBinding(Label.TextProperty, "Envelope.Description");
                        leftSecondaryLabel.SetBinding(Label.TextProperty, "Account.Description");
                        rightSingleLabel.SetBinding(Label.TextProperty, "Amount", converter: (IValueConverter)Application.Current.Resources["CurrencyConverter"]);
                        break;
                    case TransactionViewCellType.Full:
                        rightSingleLabel.IsVisible = false;
                        rightPrimaryLabel.IsVisible = true;
                        rightSecondaryLabel.IsVisible = true;
                        leftPrimaryLabel.SetBinding(Label.TextProperty, "Payee.Description");
                        leftSecondaryLabel.SetBinding(Label.TextProperty, "Account.Description");
                        rightPrimaryLabel.SetBinding(Label.TextProperty, "Amount", converter: (IValueConverter)Application.Current.Resources["CurrencyConverter"]);
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
                (bindable as TransactionViewCell).TransactionStatusButton.ToggleCommand = newValue as ICommand;
            }
        }
    }
}
