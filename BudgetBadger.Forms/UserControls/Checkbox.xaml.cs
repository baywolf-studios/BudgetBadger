using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Forms.Style;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class Checkbox : Grid
    {
        public static BindableProperty LabelProperty =
            BindableProperty.Create(nameof(Label),
                typeof(string),
                typeof(Checkbox),
                propertyChanged: (bindable, oldVal, newVal) =>
                {
                    if (bindable is Checkbox checkBox && oldVal != newVal)
                    {
                        if (string.IsNullOrEmpty((string)newVal))
                        {
                            checkBox.LabelControl.IsVisible = false;
                        }
                        else
                        {
                            checkBox.LabelControl.IsVisible = true;
                        }
                    }
                });
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static BindableProperty CaptionProperty =
            BindableProperty.Create(nameof(Caption),
                typeof(string),
                typeof(Checkbox),
                propertyChanged: (bindable, oldVal, newVal) =>
                {
                    if (bindable is Checkbox checkBox && oldVal != newVal)
                    {
                        if (string.IsNullOrEmpty((string)newVal))
                        {
                            checkBox.CaptionControl.IsVisible = false;
                            Grid.SetColumnSpan(checkBox.SwitchControl, 1);
                        }
                        else
                        {
                            checkBox.CaptionControl.IsVisible = true;
                            Grid.SetColumnSpan(checkBox.SwitchControl, 2);
                        }
                    }
                });
        public string Caption
        {
            get => (string)GetValue(CaptionProperty);
            set => SetValue(CaptionProperty, value);
        }

        public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(nameof(IsChecked), typeof(bool), typeof(Checkbox), false, BindingMode.TwoWay);
        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public static BindableProperty HintProperty = BindableProperty.Create(nameof(Hint), typeof(string), typeof(Checkbox), propertyChanged: UpdateErrorAndHint);
        public string Hint
        {
            get => (string)GetValue(HintProperty);
            set => SetValue(HintProperty, value);
        }

        public static BindableProperty ErrorProperty = BindableProperty.Create(nameof(Error), typeof(string), typeof(Checkbox), propertyChanged: UpdateErrorAndHint);
        public string Error
        {
            get => (string)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }

        private readonly bool _compact;

        public Checkbox() : this(false) { }

        public Checkbox(bool compact)
        {
            InitializeComponent();

            if (compact)
            {
                CaptionControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlLabelCompactStyle"];
                LabelControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlDescriptionLabelCompactStyle"];
                HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlHintLabelCompactStyle"];
            }
            else
            {
                CaptionControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlLabelStyle"];
                LabelControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlDescriptionLabelStyle"];
                HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlHintLabelStyle"];
            }

            _compact = compact;

            ButtonBackground.BindingContext = this;
            LabelControl.BindingContext = this;
            SwitchControl.BindingContext = this;
            CaptionControl.BindingContext = this;
        

            PropertyChanged += (sender, e) => 
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    SwitchControl.IsEnabled = IsEnabled;
                }
            };
        }

        void Handle_Clicked(object sender, EventArgs e)
        {
            if (IsEnabled && !SwitchControl.IsFocused)
            {
                SwitchControl.IsChecked = !SwitchControl.IsChecked;
            }
        }

        static void UpdateErrorAndHint(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is Checkbox checkbox && oldValue != newValue)
            {
                if (!String.IsNullOrEmpty(checkbox.Error))
                {
                    checkbox.HintErrorControl.IsVisible = true;
                    checkbox.HintErrorControl.Text = checkbox.Error;
                    if (checkbox._compact)
                    {
                        checkbox.HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlErrorLabelCompactStyle"];
                    }
                    else
                    {
                        checkbox.HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlErrorLabelCompactStyle"];
                    }
                }
                else if (!String.IsNullOrEmpty(checkbox.Hint))
                {
                    checkbox.HintErrorControl.IsVisible = true;
                    checkbox.HintErrorControl.Text = checkbox.Hint;
                    if (checkbox._compact)
                    {
                        checkbox.HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlHintLabelCompactStyle"];
                    }
                    else
                    {
                        checkbox.HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlHintLabelCompactStyle"];
                    }
                }
                else
                {
                    checkbox.HintErrorControl.IsVisible = false;
                }
            }
        }
    }
}
