using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class TransactionDetailedViewCell : ViewCell
    {
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create("Command", typeof(ICommand), typeof(TransactionDetailedViewCell), null, propertyChanged: OnCommandPropertyChanged);

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public TransactionDetailedViewCell()
        {
            InitializeComponent();
        }

        static void OnCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != oldValue)
            {
                (bindable as TransactionDetailedViewCell).hiddenButton.Command = newValue as ICommand;
            }
        }
    }
}
