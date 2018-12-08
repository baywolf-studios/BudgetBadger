using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.Pages
{
    public partial class ChildHeader : Grid
    {
        public static BindableProperty PageTitleProperty = BindableProperty.Create(nameof(PageTitle), typeof(string), typeof(ChildHeader));
        public string PageTitle
        {
            get => (string)GetValue(PageTitleProperty);
            set => SetValue(PageTitleProperty, value);
        }

        public static BindableProperty ToolbarItemTextProperty = BindableProperty.Create(nameof(ToolbarItemText), typeof(string), typeof(ChildHeader), defaultBindingMode: BindingMode.TwoWay);
        public string ToolbarItemText
        {
            get => (string)GetValue(ToolbarItemTextProperty);
            set => SetValue(ToolbarItemTextProperty, value);
        }

        public ImageSource ToolbarItemIcon
        {
            get => ToolbarItemImage.Source;
            set { ToolbarItemImage.ReplaceStringMap = ReplaceColor; ToolbarItemImage.Source = value; }
        }

        public static BindableProperty ToolbarItemCommandProperty = BindableProperty.Create(nameof(ToolbarItemCommand), typeof(ICommand), typeof(ChildHeader), defaultBindingMode: BindingMode.TwoWay);
        public ICommand ToolbarItemCommand
        {
            get => (ICommand)GetValue(ToolbarItemCommandProperty);
            set => SetValue(ToolbarItemCommandProperty, value);
        }

        public static BindableProperty BackCommandProperty = BindableProperty.Create(nameof(BackCommand), typeof(ICommand), typeof(ChildHeader), defaultBindingMode: BindingMode.TwoWay);
        public ICommand BackCommand
        {
            get => (ICommand)GetValue(BackCommandProperty);
            set => SetValue(BackCommandProperty, value);
        }

        public Dictionary<string, string> ReplaceColor
        {
            get => new Dictionary<string, string> { { "#ffffff", "#FFFFFF" } };
        }

        public ChildHeader()
        {
            InitializeComponent();
            LabelControl.BindingContext = this;
            ToolbarItemFrame.BindingContext = this;
            ToolbarItemImage.BindingContext = this;
            BackButtonFrame.BindingContext = this;
            BackButtonImage.BindingContext = this;
        }
    }
}
