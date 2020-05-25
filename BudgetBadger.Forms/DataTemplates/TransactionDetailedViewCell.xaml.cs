using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using BudgetBadger.Models;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class TransactionDetailedViewCell : Grid
    {
        private Transaction _transaction { get; set; }

        public static readonly BindableProperty AccountsProperty = BindableProperty.Create(nameof(Accounts), typeof(IList), typeof(TransactionDetailedViewCell), null);
        public IList Accounts
        {
            get => (IList)GetValue(AccountsProperty);
            set => SetValue(AccountsProperty, value);
        }

        public static readonly BindableProperty PayeesProperty = BindableProperty.Create(nameof(Payees), typeof(IList), typeof(TransactionDetailedViewCell), null);
        public IList Payees
        {
            get => (IList)GetValue(PayeesProperty);
            set => SetValue(PayeesProperty, value);
        }

        public static readonly BindableProperty EnvelopesProperty = BindableProperty.Create(nameof(Envelopes), typeof(IList), typeof(TransactionDetailedViewCell), null);
        public IList Envelopes
        {
            get => (IList)GetValue(EnvelopesProperty);
            set => SetValue(EnvelopesProperty, value);
        }

        public static BindableProperty SaveCommandProperty = BindableProperty.Create(nameof(SaveCommand), typeof(ICommand), typeof(TransactionDetailedViewCell));
        public ICommand SaveCommand
        {
            get => (ICommand)GetValue(SaveCommandProperty);
            set => SetValue(SaveCommandProperty, value);
        }

        public static BindableProperty CancelCommandProperty = BindableProperty.Create(nameof(CancelCommand), typeof(ICommand), typeof(TransactionDetailedViewCell));
        public ICommand CancelCommand
        {
            get => (ICommand)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }

        public static readonly BindableProperty ToggleCommandProperty =
            BindableProperty.Create(nameof(ToggleCommand), typeof(ICommand), typeof(TransactionViewCell), null, propertyChanged: OnToggleCommandPropertyChanged);

        public ICommand ToggleCommand
        {
            get { return (ICommand)GetValue(ToggleCommandProperty); }
            set { SetValue(ToggleCommandProperty, value); }
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
                        SetColumnSpan(divider, 7);
                        SetColumn(outflowControl, 3);
                        SetColumn(inflowControl, 4);
                        SetColumn(TransactionStatusButton, 5);
                        SetColumn(EditButton, 6);
                        SetColumn(SaveCancelContainer, 6);
                        ColumnDefinitions.Remove(envelopeColumn);
                        break;
                    case TransactionViewCellType.Account:
                        Children.Remove(accountControl);
                        SetColumn(envelopeControl, 1);
                        SetColumn(payeeControl, 2);
                        SetColumnSpan(divider, 7);
                        SetColumn(outflowControl, 3);
                        SetColumn(inflowControl, 4);
                        SetColumn(TransactionStatusButton, 5);
                        SetColumn(EditButton, 6);
                        SetColumn(SaveCancelContainer, 6);
                        ColumnDefinitions.Remove(accountColumn);
                        break;
                    case TransactionViewCellType.Payee:
                        Children.Remove(payeeControl);
                        SetColumn(accountControl, 1);
                        SetColumn(envelopeControl, 2);
                        SetColumnSpan(divider, 7);
                        SetColumn(outflowControl, 3);
                        SetColumn(inflowControl, 4);
                        SetColumn(TransactionStatusButton, 5);
                        SetColumn(EditButton, 6);
                        SetColumn(SaveCancelContainer, 6);
                        ColumnDefinitions.Remove(payeeColumn);
                        break;
                }
            }
        }

        public TransactionDetailedViewCell()
        {
            InitializeComponent();
        }

        static void OnToggleCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != oldValue)
            {
                (bindable as TransactionDetailedViewCell).TransactionStatusButton.ToggleCommand = newValue as ICommand;
            }
        }

        void Handle_EditClicked(object sender, EventArgs e)
        {
            if (BindingContext is Transaction tran)
            {
                _transaction = tran.DeepCopy();
            }

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
            accountControl.IsReadOnly = true;
            envelopeControl.IsReadOnly = true;
            payeeControl.IsReadOnly = true;
            serviceDateControl.IsReadOnly = true;
            outflowControl.IsReadOnly = true;
            inflowControl.IsReadOnly = true;

            if (SaveCommand?.CanExecute(BindingContext) ?? false)
            {
                SaveCommand?.Execute(BindingContext);
            }

            _transaction = null;
        }

        void Handle_CancelClicked(object sender, EventArgs e)
        {
            EditButton.IsVisible = true;
            SaveCancelContainer.IsVisible = false;
            accountControl.IsReadOnly = true;
            envelopeControl.IsReadOnly = true;
            payeeControl.IsReadOnly = true;
            serviceDateControl.IsReadOnly = true;
            outflowControl.IsReadOnly = true;
            inflowControl.IsReadOnly = true;

            if (CancelCommand?.CanExecute(_transaction) ?? false)
            {
                CancelCommand?.Execute(_transaction);
            }

            _transaction = null;
        }
    }
}
