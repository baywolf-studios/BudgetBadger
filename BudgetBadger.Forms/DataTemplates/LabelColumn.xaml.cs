using System;
using System.Collections.Generic;
using System.Windows.Input;
using BudgetBadger.Forms.UserControls;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class LabelColumn  : ContentButton
    {
        public TextAlignment HorizontalTextAlignment
        {
            get => TextControl.HorizontalTextAlignment;
            set => TextControl.HorizontalTextAlignment = value;
        }

        public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(TextColumn), defaultBindingMode: BindingMode.TwoWay);
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static BindableProperty SelectedCommandProperty = BindableProperty.Create(nameof(SelectedCommand), typeof(ICommand), typeof(TextColumn));
        public ICommand SelectedCommand
        {
            get => (ICommand)GetValue(SelectedCommandProperty);
            set => SetValue(SelectedCommandProperty, value);
        }

        public static BindableProperty SelectedCommandParameterProperty = BindableProperty.Create(nameof(SelectedCommandParameter), typeof(object), typeof(TextColumn));
        public object SelectedCommandParameter
        {
            get => GetValue(SelectedCommandParameterProperty);
            set => SetValue(SelectedCommandParameterProperty, value);
        }

        public LabelColumn() : this(false) { }

        public LabelColumn(bool dense)
        {
            InitializeComponent();
            if (dense)
            {
                if (Application.Current.Resources.TryGetValue("DenseDataTableLabelColumnCellStyle", out object resource))
                {
                    TextControl.Style = (Xamarin.Forms.Style)resource;
                }
            }
            else
            {
                if (Application.Current.Resources.TryGetValue("DataTableLabelColumnCellStyle", out object resource))
                {
                    TextControl.Style = (Xamarin.Forms.Style)resource;
                }
            }
            TextControl.BindingContext = this;
        }

        void Handle_Tapped(object sender, System.EventArgs e)
        {
            if (SelectedCommand != null && SelectedCommand.CanExecute(SelectedCommandParameter))
            {
                SelectedCommand.Execute(SelectedCommandParameter);
            }
        }
    }
}
