using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class TransactionDetailedViewCell : ViewCell
    {
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create("Command", typeof(ICommand), typeof(TransactionDetailedViewCell), null, propertyChanged: OnCommandPropertyChanged);

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
                switch (transactionViewCellType)
                {
                    case TransactionViewCellType.Envelope:
                        transactionGrid.ColumnDefinitions.Clear();
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(48, GridUnitType.Absolute) });

                        transactionGrid.Children.Clear();
                        transactionGrid.Children.Add(serviceDateLabel, 0, 0);
                        transactionGrid.Children.Add(accountLabel, 1, 0);
                        transactionGrid.Children.Add(payeeLabel, 2, 0);
                        transactionGrid.Children.Add(amountLabel, 3, 0);
                        transactionGrid.Children.Add(clearedImage, 4, 0);
                        transactionGrid.Children.Add(hiddenButton, 4, 0);
                        break;
                    case TransactionViewCellType.Account:
                        transactionGrid.ColumnDefinitions.Clear();
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(48, GridUnitType.Absolute) });

                        transactionGrid.Children.Clear();
                        transactionGrid.Children.Add(serviceDateLabel, 0, 0);
                        transactionGrid.Children.Add(payeeLabel, 1, 0);
                        transactionGrid.Children.Add(envelopeLabel, 2, 0);
                        transactionGrid.Children.Add(amountLabel, 3, 0);
                        transactionGrid.Children.Add(clearedImage, 4, 0);
                        transactionGrid.Children.Add(hiddenButton, 4, 0);
                        break;
                    case TransactionViewCellType.Payee:
                        transactionGrid.ColumnDefinitions.Clear();
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(48, GridUnitType.Absolute) });

                        transactionGrid.Children.Clear();
                        transactionGrid.Children.Add(serviceDateLabel, 0, 0);
                        transactionGrid.Children.Add(accountLabel, 1, 0);
                        transactionGrid.Children.Add(envelopeLabel, 2, 0);
                        transactionGrid.Children.Add(amountLabel, 3, 0);
                        transactionGrid.Children.Add(clearedImage, 4, 0);
                        transactionGrid.Children.Add(hiddenButton, 4, 0);
                        break;
                    case TransactionViewCellType.Full:
                        transactionGrid.ColumnDefinitions.Clear();
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        transactionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(48, GridUnitType.Absolute) });

                        transactionGrid.Children.Clear();
                        transactionGrid.Children.Add(serviceDateLabel, 0, 0);
                        transactionGrid.Children.Add(accountLabel, 1, 0);
                        transactionGrid.Children.Add(payeeLabel, 2, 0);
                        transactionGrid.Children.Add(envelopeLabel, 3, 0);
                        transactionGrid.Children.Add(amountLabel, 4, 0);
                        transactionGrid.Children.Add(clearedImage, 5, 0);
                        transactionGrid.Children.Add(hiddenButton, 5, 0);
                        break;
                }
            }
        }

        public TransactionDetailedViewCell()
        {
            InitializeComponent();
        }

        static void OnCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != oldValue)
            {
                (bindable as TransactionDetailedViewCell).hiddenButton.Command = newValue as ICommand;
            }
        }
    }
}
