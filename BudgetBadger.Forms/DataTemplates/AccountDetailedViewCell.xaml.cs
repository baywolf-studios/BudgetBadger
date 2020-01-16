using System;
using System.Collections.Generic;
using System.Windows.Input;
using BudgetBadger.Forms.UserControls;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class AccountDetailedViewCell : SelectableViewCell
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

        public AccountDetailedViewCell()
        {
            InitializeComponent();
        }

        void Handle_Clicked(object sender, EventArgs e)
        {
            DescriptionControl.IsReadOnly = !DescriptionControl.IsReadOnly;

        }
    }
}
