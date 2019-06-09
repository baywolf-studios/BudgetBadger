using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows.Input;
using BudgetBadger.Core.LocalizedResources;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class CurrencyCalculatorColumn : ContentView
    {
        public static BindableProperty NumberProperty = BindableProperty.Create(nameof(Number), typeof(decimal?), typeof(CurrencyCalculatorColumn), defaultBindingMode: BindingMode.TwoWay);
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

        public CurrencyCalculatorColumn()
        {
            InitializeComponent();
            TextControl.BindingContext = this;

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    TextControl.IsEnabled = IsEnabled;
                }
            };

            TextControl.Unfocused += (sender, e) =>
            {
                BackgroundColor = Color.Transparent;

                if (SaveCommand != null && SaveCommand.CanExecute(SaveCommandParameter))
                {
                    SaveCommand.Execute(SaveCommandParameter);
                }
            };

            TextControl.Focused += (sender, e) =>
            {
                BackgroundColor = (Color)Application.Current.Resources["SelectedItemColor"];
            };
        }

        void Handle_Tapped(object sender, System.EventArgs e)
        {
            if (!TextControl.IsFocused)
            {
                TextControl.Focus();
            }
        }
    }
}
