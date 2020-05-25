using System;
using System.Collections.Generic;
using System.Windows.Input;
using BudgetBadger.Forms.UserControls;
using BudgetBadger.Models;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class EnvelopeDetailedViewCell : Grid
    {
        private Budget _budget { get; set; }

        public static BindableProperty SaveCommandProperty = BindableProperty.Create(nameof(SaveCommand), typeof(ICommand), typeof(EnvelopeDetailedViewCell));
        public ICommand SaveCommand
        {
            get => (ICommand)GetValue(SaveCommandProperty);
            set => SetValue(SaveCommandProperty, value);
        }

        public static BindableProperty CancelCommandProperty = BindableProperty.Create(nameof(CancelCommand), typeof(ICommand), typeof(EnvelopeDetailedViewCell));
        public ICommand CancelCommand
        {
            get => (ICommand)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }

        public static BindableProperty IsReadOnlyProperty = BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(EnvelopeDetailedViewCell));
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public EnvelopeDetailedViewCell()
        {
            InitializeComponent();
        }

        void Handle_EditClicked(object sender, EventArgs e)
        {
            if (BindingContext is Budget b)
            {
                _budget = b.DeepCopy();
            }

            DescriptionControl.IsReadOnly = false;
            AmountControl.IsReadOnly = false;
            EditButton.IsVisible = false;
            SaveCancelContainer.IsVisible = true;
        }

        void Handle_SaveClicked(object sender, EventArgs e)
        {
            DescriptionControl.IsReadOnly = true;
            AmountControl.IsReadOnly = true;
            EditButton.IsVisible = true;
            SaveCancelContainer.IsVisible = false;

            if (SaveCommand?.CanExecute(BindingContext) ?? false)
            {
                SaveCommand?.Execute(BindingContext);
            }

            _budget = null;
        }

        void Handle_CancelClicked(object sender, EventArgs e)
        {
            DescriptionControl.IsReadOnly = true;
            AmountControl.IsReadOnly = true;
            EditButton.IsVisible = true;
            SaveCancelContainer.IsVisible = false;

            if (CancelCommand?.CanExecute(_budget) ?? false)
            {
                CancelCommand?.Execute(_budget);
            }

            _budget = null;
        }
    }
}
