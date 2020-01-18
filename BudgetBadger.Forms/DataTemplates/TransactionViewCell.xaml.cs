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
                        primaryLeftSpan.SetBinding(Span.TextProperty, "Payee.Description");
                        newLineLeftSpan.Text = Environment.NewLine;
                        secondaryLeftSpan.SetBinding(Span.TextProperty, "Account.Description");
                        primaryRightSpan.SetBinding(Span.TextProperty, "Amount", converter: (IValueConverter)Application.Current.Resources["CurrencyConverter"]);
                        newLineRightSpan.Text = "";
                        secondaryRightSpan.Text = "";
                        break;
                    case TransactionViewCellType.Account:
                        primaryLeftSpan.SetBinding(Span.TextProperty, "Payee.Description");
                        newLineLeftSpan.Text = Environment.NewLine;
                        secondaryLeftSpan.SetBinding(Span.TextProperty, "Envelope.Description");
                        primaryRightSpan.SetBinding(Span.TextProperty, "Amount", converter: (IValueConverter)Application.Current.Resources["CurrencyConverter"]);
                        newLineRightSpan.Text = "";
                        secondaryRightSpan.Text = "";
                        break;
                    case TransactionViewCellType.Payee:
                        primaryLeftSpan.SetBinding(Span.TextProperty, "Envelope.Description");
                        newLineLeftSpan.Text = Environment.NewLine;
                        secondaryLeftSpan.SetBinding(Span.TextProperty, "Account.Description");
                        primaryRightSpan.SetBinding(Span.TextProperty, "Amount", converter: (IValueConverter)Application.Current.Resources["CurrencyConverter"]);
                        newLineRightSpan.Text = "";
                        secondaryRightSpan.Text = "";
                        break;
                    case TransactionViewCellType.Full:
                        primaryLeftSpan.SetBinding(Span.TextProperty, "Payee.Description");
                        newLineLeftSpan.Text = Environment.NewLine;
                        secondaryLeftSpan.SetBinding(Span.TextProperty, "Account.Description");
                        primaryRightSpan.SetBinding(Span.TextProperty, "Amount", converter: (IValueConverter)Application.Current.Resources["CurrencyConverter"]);
                        newLineRightSpan.Text = Environment.NewLine;
                        secondaryRightSpan.SetBinding(Span.TextProperty, "Envelope.Description");
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
