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

        public AccountDetailedViewCell()
        {
            InitializeComponent();
            DescriptionControl.Completed += DescriptionControl_Completed;
        }

        private void DescriptionControl_Completed(object sender, EventArgs e)
        {
            if (SaveCommand != null && SaveCommand.CanExecute(BindingContext))
            {
                SaveCommand.Execute(BindingContext);
            }
        }
    }
}
