using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows.Input;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Forms.UserControls;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class CurrencyCalculatorColumn : ContentButton
    {
        public static BindableProperty IsReadOnlyProperty = BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(TextColumn));
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public static BindableProperty NumberProperty = BindableProperty.Create(nameof(Number), typeof(decimal?), typeof(CurrencyCalculatorColumn), defaultBindingMode: BindingMode.TwoWay, propertyChanged: (bindable, oldVal, newVal) =>
        {
            ((CurrencyCalculatorColumn)bindable).TextControl.Number = (decimal?)newVal;
        });
        public decimal? Number
        {
            get => (decimal?)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }

        public static BindableProperty SaveCommandProperty = BindableProperty.Create(nameof(SaveCommand), typeof(ICommand), typeof(CurrencyCalculatorColumn));
        public ICommand SaveCommand
        {
            get => (ICommand)GetValue(SaveCommandProperty);
            set => SetValue(SaveCommandProperty, value);
        }

        public static BindableProperty SaveCommandParameterProperty = BindableProperty.Create(nameof(SaveCommandParameter), typeof(object), typeof(CurrencyCalculatorColumn));
        public object SaveCommandParameter
        {
            get => GetValue(SaveCommandParameterProperty);
            set => SetValue(SaveCommandParameterProperty, value);
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

        public CurrencyCalculatorColumn() : this(false) { }

        public CurrencyCalculatorColumn(bool dense)
        {
            InitializeComponent();
            if(dense)
            {
                TextControl.FontSize = (double)Application.Current.Resources["DataGridItemDenseFontSize"];
            }
            else
            {
                TextControl.FontSize = (double)Application.Current.Resources["DataGridItemFontSize"];
            }

            TextControl.BindingContext = this;

            TextControl.Focused += Control_Focused;
            TextControl.Unfocused += TextControl_Completed;
            TextControl.Completed += TextControl_Completed;
        }

        void Control_Focused(object sender, FocusEventArgs e)
        {
            if (IsReadOnly || !IsEnabled)
            {
                TextControl.Unfocus();
            }

            UpdateActive();
        }

        void TextControl_Completed(object sender, EventArgs e)
        {
            if (Number != TextControl.Number)
            {
                Number = TextControl.Number;
                if (SaveCommand != null && SaveCommand.CanExecute(SaveCommandParameter))
                {
                    SaveCommand.Execute(SaveCommandParameter);
                }
            }

            ForceActiveBackground = false;
            ForceActiveBackground = true;
        }

        void Handle_Tapped(object sender, System.EventArgs e)
        {
            if (IsReadOnly || !IsEnabled)
            {
                if (SelectedCommand != null && SelectedCommand.CanExecute(SelectedCommandParameter))
                {
                    SelectedCommand.Execute(SelectedCommandParameter);
                }
                ForceActiveBackground = false;
                ForceActiveBackground = true;
            }
            else if (!TextControl.IsFocused)
            {
                TextControl.Focus();
            }
        }
    }
}
