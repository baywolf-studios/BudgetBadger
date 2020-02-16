using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class PayeeDetailedViewCell : Grid
    {
        public static BindableProperty SaveCommandProperty = BindableProperty.Create(nameof(SaveCommand), typeof(ICommand), typeof(PayeeDetailedViewCell));
        public ICommand SaveCommand
        {
            get => (ICommand)GetValue(SaveCommandProperty);
            set => SetValue(SaveCommandProperty, value);
        }

        public static BindableProperty RefreshItemCommandProperty = BindableProperty.Create(nameof(RefreshItemCommand), typeof(ICommand), typeof(PayeeDetailedViewCell));
        public ICommand RefreshItemCommand
        {
            get => (ICommand)GetValue(RefreshItemCommandProperty);
            set => SetValue(RefreshItemCommandProperty, value);
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
        }

        void Handle_CancelClicked(object sender, EventArgs e)
        {
            DescriptionControl.IsReadOnly = true;
            NotesControl.IsReadOnly = true;
            EditButton.IsVisible = true;
            SaveCancelContainer.IsVisible = false;
            if (RefreshItemCommand?.CanExecute(BindingContext) ?? false)
            {
                RefreshItemCommand?.Execute(BindingContext);
            }
        }
    }
}
