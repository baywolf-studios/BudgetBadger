﻿using System;
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
