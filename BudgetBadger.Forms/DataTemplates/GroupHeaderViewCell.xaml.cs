using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class GroupHeaderViewCell : ViewCell
    {
        //public static readonly BindableProperty CommandProperty =
        //    BindableProperty.Create("Command", typeof(ICommand), typeof(GroupHeaderViewCell), null, propertyChanged: OnParentContextPropertyChanged);

        //public ICommand Command
        //{
        //    get { return GetValue(CommandProperty); }
        //    set { SetValue(CommandProperty, value); }
        //}

        public GroupHeaderViewCell()
        {
            InitializeComponent();
        }

        //private static void OnParentContextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        //{
        //    if (newValue != oldValue && newValue != null)
        //    {
        //        //(bindable as GroupHeaderViewCell).Command = newValue;
        //    }
        //}
    }
}
