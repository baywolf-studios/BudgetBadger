using System;
using System.Collections.Generic;
using System.Windows.Input;
using BudgetBadger.Models;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class PayeeDetailedViewCell : Grid
    {
        private Payee _payee { get; set; }

        public static BindableProperty SaveCommandProperty = BindableProperty.Create(nameof(SaveCommand), typeof(ICommand), typeof(PayeeDetailedViewCell));
        public ICommand SaveCommand
        {
            get => (ICommand)GetValue(SaveCommandProperty);
            set => SetValue(SaveCommandProperty, value);
        }

        public static BindableProperty CancelCommandProperty = BindableProperty.Create(nameof(CancelCommand), typeof(ICommand), typeof(PayeeDetailedViewCell));
        public ICommand CancelCommand
        {
            get => (ICommand)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }

        public static BindableProperty IsReadOnlyProperty = BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(PayeeDetailedViewCell));
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public PayeeDetailedViewCell()
        {
            InitializeComponent();
        }

        void Handle_EditClicked(object sender, EventArgs e)
        {
            if (BindingContext is Payee p)
            {
                _payee = p.DeepCopy();
            }

            DescriptionControl.IsReadOnly = false;
            NotesControl.IsReadOnly = false;
            EditButton.IsVisible = false;
            SaveCancelContainer.IsVisible = true;
        }

        void Handle_SaveClicked(object sender, EventArgs e)
        {
            DescriptionControl.IsReadOnly = true;
            NotesControl.IsReadOnly = true;
            EditButton.IsVisible = true;
            SaveCancelContainer.IsVisible = false;

            if (SaveCommand?.CanExecute(BindingContext) ?? false)
            {
                SaveCommand?.Execute(BindingContext);
            }

            _payee = null;
        }

        void Handle_CancelClicked(object sender, EventArgs e)
        {
            DescriptionControl.IsReadOnly = true;
            NotesControl.IsReadOnly = true;
            EditButton.IsVisible = true;
            SaveCancelContainer.IsVisible = false;

            if (CancelCommand?.CanExecute(_payee) ?? false)
            {
                CancelCommand?.Execute(_payee);
            }

            _payee = null;
        }
    }
}
