using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class MultilineTextColumn : ContentView
    {
        public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(MultilineTextColumn), defaultBindingMode: BindingMode.TwoWay);
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static BindableProperty SaveCommandProperty = BindableProperty.Create(nameof(SaveCommand), typeof(ICommand), typeof(MultilineTextColumn));
        public ICommand SaveCommand
        {
            get => (ICommand)GetValue(SaveCommandProperty);
            set => SetValue(SaveCommandProperty, value);
        }

        public static BindableProperty SaveCommandParameterProperty = BindableProperty.Create(nameof(SaveCommandParameter), typeof(object), typeof(MultilineTextColumn));
        public object SaveCommandParameter
        {
            get => GetValue(SaveCommandParameterProperty);
            set => SetValue(SaveCommandParameterProperty, value);
        }

        public MultilineTextColumn()
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
