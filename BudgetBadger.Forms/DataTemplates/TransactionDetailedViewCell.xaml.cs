using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class TransactionDetailedViewCell : Grid
    {
        public static BindableProperty SaveCommandProperty = BindableProperty.Create(nameof(SaveCommand), typeof(ICommand), typeof(TextColumn));
        public ICommand SaveCommand
        {
            get => (ICommand)GetValue(SaveCommandProperty);
            set => SetValue(SaveCommandProperty, value);
        }

        public static BindableProperty RefreshItemCommandProperty = BindableProperty.Create(nameof(RefreshItemCommand), typeof(ICommand), typeof(TextColumn));
        public ICommand RefreshItemCommand
        {
            get => (ICommand)GetValue(RefreshItemCommandProperty);
            set => SetValue(RefreshItemCommandProperty, value);
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
                        Children.Remove(envelopeControl);
                        SetColumn(accountControl, 1);
                        SetColumn(payeeControl, 2);
                        SetColumnSpan(divider, 6);
                        SetColumn(outflowControl, 3);
                        SetColumn(inflowControl, 4);
                        SetColumn(EditButton, 5);
                        SetColumn(SaveCancelContainer, 5);
                        ColumnDefinitions.Remove(envelopeColumn);
                        break;
                    case TransactionViewCellType.Account:
                        Children.Remove(accountControl);
                        SetColumn(envelopeControl, 1);
                        SetColumn(payeeControl, 2);
                        SetColumnSpan(divider, 6);
                        SetColumn(outflowControl, 3);
                        SetColumn(inflowControl, 4);
                        SetColumn(EditButton, 5);
                        SetColumn(SaveCancelContainer, 5);
                        ColumnDefinitions.Remove(accountColumn);
                        break;
                    case TransactionViewCellType.Payee:
                        Children.Remove(payeeControl);
                        SetColumn(accountControl, 1);
                        SetColumn(envelopeControl, 2);
                        SetColumnSpan(divider, 6);
                        SetColumn(outflowControl, 3);
                        SetColumn(inflowControl, 4);
                        SetColumn(EditButton, 5);
                        SetColumn(SaveCancelContainer, 5);
                        ColumnDefinitions.Remove(payeeColumn);
                        break;
                }
            }
        }

        public TransactionDetailedViewCell()
        {
            InitializeComponent();
        }

        void Handle_EditClicked(object sender, EventArgs e)
        {
            EditButton.IsVisible = false;
            SaveCancelContainer.IsVisible = true;
        }

        void Handle_SaveClicked(object sender, EventArgs e)
        {
            EditButton.IsVisible = true;
            SaveCancelContainer.IsVisible = false;
            if (SaveCommand?.CanExecute(BindingContext) ?? false)
            {
                SaveCommand?.Execute(BindingContext);
            }
        }

        void Handle_CancelClicked(object sender, EventArgs e)
        {
            EditButton.IsVisible = true;
            SaveCancelContainer.IsVisible = false;
            if (RefreshItemCommand?.CanExecute(BindingContext) ?? false)
            {
                RefreshItemCommand?.Execute(BindingContext);
            }
        }
    }
}
