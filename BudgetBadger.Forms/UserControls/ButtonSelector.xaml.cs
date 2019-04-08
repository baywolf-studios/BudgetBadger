using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Forms.Animation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class ButtonSelector : StackLayout
    {
        public static BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(ButtonSelector));
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(ButtonSelector), propertyChanged: (bindable, oldval, newval) =>

        {
            if (oldval != newval)
            {
                if (newval != null)
                {
                    (bindable as ButtonSelector).PickerControl.ItemsSource = new List<string>() { newval.ToString() };
                    (bindable as ButtonSelector).PickerControl.SelectedIndex = 0;
                    (bindable as ButtonSelector).TextControl.Text = newval.ToString();
                }
                else
                {
                    (bindable as ButtonSelector).PickerControl.ItemsSource = new List<string>();
                    (bindable as ButtonSelector).PickerControl.SelectedIndex = -1;
                    (bindable as ButtonSelector).TextControl.Text = "";
                }
            }
        });
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ButtonSelector));
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ButtonSelector));
        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public ButtonSelector()
        {
            InitializeComponent();

            LabelControl.BindingContext = this;
            PickerContentView.BindingContext = this;
            TextControl.BindingContext = this;

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    PickerControl.IsEnabled = IsEnabled;
                    TextControl.IsEnabled = IsEnabled;
                }

                if (e.PropertyName == nameof(IsFocused))
                {
                    if (IsFocused)
                    {
                        PickerControl.Unfocus();
                    }
                }
            };

            PickerControl.Focused += (sender, e) =>
            {
                if (Command != null)
                {
                    Command.Execute(CommandParameter);
                }
                else
                {
                    PickerControl.Unfocus();
                }
            };
        }
    }
}
