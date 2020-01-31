using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class TransactionDetailedViewCell : Grid
    {
        public static readonly BindableProperty AccountsProperty = BindableProperty.Create(nameof(Accounts), typeof(IEnumerable), typeof(TransactionDetailedViewCell), null);
        public IEnumerable Accounts
        {
            get => (IEnumerable)GetValue(AccountsProperty);
            set => SetValue(AccountsProperty, value);
        }

        public static readonly BindableProperty PayeesProperty = BindableProperty.Create(nameof(Payees), typeof(IEnumerable), typeof(TransactionDetailedViewCell), null);
        public IEnumerable Payees
        {
            get => (IEnumerable)GetValue(PayeesProperty);
            set => SetValue(PayeesProperty, value);
        }

        public static readonly BindableProperty EnvelopesProperty = BindableProperty.Create(nameof(Envelopes), typeof(IEnumerable), typeof(TransactionDetailedViewCell), null);
        public IEnumerable Envelopes
        {
            get => (IEnumerable)GetValue(EnvelopesProperty);
            set => SetValue(EnvelopesProperty, value);
        }

        public static BindableProperty SaveCommandProperty = BindableProperty.Create(nameof(SaveCommand), typeof(ICommand), typeof(TransactionDetailedViewCell));
        public ICommand SaveCommand
        {
            get => (ICommand)GetValue(SaveCommandProperty);
            set => SetValue(SaveCommandProperty, value);
        }

        public static BindableProperty RefreshItemCommandProperty = BindableProperty.Create(nameof(RefreshItemCommand), typeof(ICommand), typeof(TransactionDetailedViewCell));
        public ICommand RefreshItemCommand
        {
            get => (ICommand)GetValue(RefreshItemCommandProperty);
            set => SetValue(RefreshItemCommandProperty, value);
        }

        public static BindableProperty IsReadOnlyProperty = BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(TransactionDetailedViewCell));
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
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
            accountControl.IsReadOnly = false;
            envelopeControl.IsReadOnly = false;
            payeeControl.IsReadOnly = false;
            serviceDateControl.IsReadOnly = false;
            outflowControl.IsReadOnly = false;
            inflowControl.IsReadOnly = false;
        }

        void Handle_SaveClicked(object sender, EventArgs e)
        {
            EditButton.IsVisible = true;
            SaveCancelContainer.IsVisible = false;
            if (SaveCommand?.CanExecute(BindingContext) ?? false)
            {
                SaveCommand?.Execute(BindingContext);
            }

            accountControl.IsReadOnly = true;
            envelopeControl.IsReadOnly = true;
            payeeControl.IsReadOnly = true;
            serviceDateControl.IsReadOnly = true;
            outflowControl.IsReadOnly = true;
            inflowControl.IsReadOnly = true;
        }

        void Handle_CancelClicked(object sender, EventArgs e)
        {
            EditButton.IsVisible = true;
            SaveCancelContainer.IsVisible = false;
            if (RefreshItemCommand?.CanExecute(BindingContext) ?? false)
            {
                RefreshItemCommand?.Execute(BindingContext);
            }

            accountControl.IsReadOnly = true;
            envelopeControl.IsReadOnly = true;
            payeeControl.IsReadOnly = true;
            serviceDateControl.IsReadOnly = true;
            outflowControl.IsReadOnly = true;
            inflowControl.IsReadOnly = true;
        }
    }
}
