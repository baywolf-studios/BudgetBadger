using System;
using System.Collections.Generic;
using System.Windows.Input;
using BudgetBadger.Models;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class TransactionStatusButton : ContentView
    {
        public static BindableProperty TransactionProperty =
            BindableProperty.Create(nameof(Transaction),
                typeof(Transaction),
                typeof(TransactionStatusButton),
                null,
                propertyChanged: OnTransactionPropertyChanged);
        public Transaction Transaction
        {
            get => (Transaction)GetValue(TransactionProperty);
            set => SetValue(TransactionProperty, value);
        }

        public static readonly BindableProperty ToggleCommandProperty =
            BindableProperty.Create(nameof(ToggleCommand),
                typeof(ICommand),
                typeof(TransactionStatusButton),
                null,
                propertyChanged: OnToggleCommandPropertyChanged);
        public ICommand ToggleCommand
        {
            get { return (ICommand)GetValue(ToggleCommandProperty); }
            set { SetValue(ToggleCommandProperty, value); }
        }

        public TransactionStatusButton()
        {
            InitializeComponent();

        }

        static void OnToggleCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != oldValue)
            {
                (bindable as TransactionStatusButton).ToggleGestureRecognizer.Command = newValue as ICommand;
            }
        }

        static void OnTransactionPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != oldValue)
            {
                (bindable as TransactionStatusButton).ToggleGestureRecognizer.CommandParameter = newValue;
            }
        }
    }
}
