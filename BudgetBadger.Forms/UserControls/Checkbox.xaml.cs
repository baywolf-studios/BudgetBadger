using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class Checkbox : AbsoluteLayout
    {
        uint _animationLength = 150;

        Color _disabledColor
        {
            get => (Color)Application.Current.Resources["DisabledColor"];
        }

        Color _idleColor
        {
            get => (Color)Application.Current.Resources["IdleColor"];
        }

        Color _focusedColor
        {
            get => (Color)Application.Current.Resources["PrimaryColor"];
        }

        public static BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(Checkbox), defaultBindingMode: BindingMode.TwoWay);
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(nameof(IsChecked), typeof(bool), typeof(Checkbox), false, BindingMode.TwoWay);
        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public Dictionary<string, string> ReplaceColorUnchecked
        {
            get
            {
                if (IsEnabled)
                {
                    return new Dictionary<string, string> { { "currentColor", _idleColor.GetHexString() } };
                }
                else
                {
                    return new Dictionary<string, string> { { "currentColor", _disabledColor.GetHexString() } };
                }
            }
        }

        public Dictionary<string, string> ReplaceColorChecked
        {
            get
            {
                if (IsEnabled)
                {
                    return new Dictionary<string, string> { { "currentColor", _focusedColor.GetHexString() } };
                }
                else
                {
                    return new Dictionary<string, string> { { "currentColor", _disabledColor.GetHexString() } };
                }
            }
        }

        public Checkbox()
        {
            InitializeComponent();
            LabelControl.BindingContext = this;
            UncheckedImage.BindingContext = this;
            CheckedImage.BindingContext = this;

            UpdateVisualState();

            PropertyChanged += (sender, e) => 
            {
                if (e.PropertyName == nameof(IsEnabled) || e.PropertyName == nameof(IsChecked) || e.PropertyName == nameof(IsFocused))
                {
                    UpdateVisualState();
                }
            };
        }

        void Handle_Tapped(object sender, System.EventArgs e)
        {
            if (IsEnabled)
            {
                IsChecked = !IsChecked;
            }

            //UpdateVisualState();
        }

        async void UpdateVisualState()
        {
            var tasks = new List<Task>();

            if (IsEnabled)
            {
                if (IsChecked)
                {
                    // show the checked image
                    tasks.Add(CheckedImage.FadeTo(1, _animationLength, Easing.CubicInOut));

                    // hide the unchecked image
                    tasks.Add(UncheckedImage.FadeTo(0, _animationLength, Easing.CubicInOut));
                }
                else
                {
                    // show the unchecked image
                    tasks.Add(UncheckedImage.FadeTo(1, _animationLength, Easing.CubicInOut));

                    // hide the checked image
                    tasks.Add(CheckedImage.FadeTo(0, _animationLength, Easing.CubicInOut));
                }
            }
            else
            {
                if (IsChecked)
                {
                    // show the checked image
                    tasks.Add(CheckedImage.FadeTo(1, _animationLength, Easing.CubicInOut));

                    // hide the unchecked image
                    tasks.Add(UncheckedImage.FadeTo(0, _animationLength, Easing.CubicInOut));
                }
                else
                {
                    // show the unchecked image
                    tasks.Add(UncheckedImage.FadeTo(1, _animationLength, Easing.CubicInOut));

                    // hide the checked image
                    tasks.Add(CheckedImage.FadeTo(0, _animationLength, Easing.CubicInOut));
                }
            }

            await Task.WhenAll(tasks);
            OnPropertyChanged(nameof(ReplaceColorUnchecked));
            OnPropertyChanged(nameof(ReplaceColorChecked));
        }
    }
}
