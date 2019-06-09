using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class DatePickerColumn : ContentView
    {
        public static BindableProperty DateProperty =
            BindableProperty.Create(nameof(Date),
                                    typeof(DateTime),
                                    typeof(DatePickerColumn),
                                    defaultValue: DateTime.Now,
                                    defaultBindingMode: BindingMode.TwoWay);
        public DateTime Date
        {
            get => (DateTime)GetValue(DateProperty);
            set => SetValue(DateProperty, value);
        }

        public static BindableProperty SaveCommandProperty = BindableProperty.Create(nameof(SaveCommand), typeof(ICommand), typeof(DatePickerColumn));
        public ICommand SaveCommand
        {
            get => (ICommand)GetValue(SaveCommandProperty);
            set => SetValue(SaveCommandProperty, value);
        }

        public static BindableProperty SaveCommandParameterProperty = BindableProperty.Create(nameof(SaveCommandParameter), typeof(object), typeof(DatePickerColumn));
        public object SaveCommandParameter
        {
            get => GetValue(SaveCommandParameterProperty);
            set => SetValue(SaveCommandParameterProperty, value);
        }

        public DatePickerColumn()
        {
            InitializeComponent();
            DateControl.BindingContext = this;

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    DateControl.IsEnabled = IsEnabled;
                }
            };

            DateControl.Unfocused += (sender, e) =>
            {
                BackgroundColor = Color.Transparent;

                if (SaveCommand != null && SaveCommand.CanExecute(SaveCommandParameter))
                {
                    SaveCommand.Execute(SaveCommandParameter);
                }
            };

            DateControl.Focused += (sender, e) =>
            {
                BackgroundColor = (Color)Application.Current.Resources["SelectedItemColor"];
            };
        }

        void Handle_Tapped(object sender, System.EventArgs e)
        {
            if (!DateControl.IsFocused)
            {
                DateControl.Focus();
            }
        }
    }
}
