using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class Checkbox : AbsoluteLayout
    {
        uint _animationLength = 150;

        double _disabledOpacity
        {
            get => (Double)Application.Current.Resources["DisabledOpacity"];
        }

        double _idleOpacity
        {
            get => (Double)Application.Current.Resources["IdleOpacity"];
        }

        double _focusedOpacity
        {
            get => (Double)Application.Current.Resources["FocusedOpacity"];
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

        public Checkbox()
        {
            InitializeComponent();
            LabelControl.BindingContext = this;
            UpdateVisualState();

            PropertyChanged += (sender, e) => 
            {
                if (e.PropertyName == nameof(IsChecked))
                {
                    UpdateVisualState();
                }
            };
        }

        void Handle_Tapped(object sender, System.EventArgs e)
        {
            IsChecked = !IsChecked;

            UpdateVisualState();
        }

        async void UpdateVisualState()
        {
            var tasks = new List<Task>();

            if (IsEnabled)
            {
                if (IsChecked)
                {
                    // show the checked image
                    tasks.Add(CheckedImage.FadeTo(_focusedOpacity, _animationLength, Easing.CubicInOut));

                    // hide the unchecked image
                    tasks.Add(UnCheckedImage.FadeTo(0, _animationLength, Easing.CubicInOut));
                }
                else
                {
                    // show the unchecked image
                    tasks.Add(UnCheckedImage.FadeTo(_idleOpacity, _animationLength, Easing.CubicInOut));

                    // hide the checked image
                    tasks.Add(CheckedImage.FadeTo(0, _animationLength, Easing.CubicInOut));
                }
            }
            else
            {
                if (IsChecked)
                {
                    // show the checked image
                    tasks.Add(CheckedImage.FadeTo(_disabledOpacity, _animationLength, Easing.CubicInOut));

                    // hide the unchecked image
                    tasks.Add(UnCheckedImage.FadeTo(0, _animationLength, Easing.CubicInOut));
                }
                else
                {
                    // show the unchecked image
                    tasks.Add(UnCheckedImage.FadeTo(_disabledOpacity, _animationLength, Easing.CubicInOut));

                    // hide the checked image
                    tasks.Add(CheckedImage.FadeTo(0, _animationLength, Easing.CubicInOut));
                }
            }

            await Task.WhenAll(tasks);
        }
    }
}
