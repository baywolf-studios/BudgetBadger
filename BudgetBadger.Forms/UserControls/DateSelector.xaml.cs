using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using BudgetBadger.Forms.Animation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class DateSelector : AbsoluteLayout
    {
        uint _animationLength = 150;

        Color _disabledColor
        {
            get => (Color)Application.Current.Resources["DisabledColor"];
        }

        Color _errorColor
        {
            get => (Color)Application.Current.Resources["ErrorColor"];
        }

        Color _idleColor
        {
            get => (Color)Application.Current.Resources["IdleColor"];
        }

        Color _focusedColor
        {
            get => (Color)Application.Current.Resources["PrimaryColor"];
        }

        Color _textColor
        {
            get => (Color)Application.Current.Resources["PrimaryTextColor"];
        }

        double _disabledImageOpacity
        {
            get => (Double)Application.Current.Resources["DisabledOpacity"];
        }

        double _idleImageOpacity
        {
            get => (Double)Application.Current.Resources["IdleOpacity"];
        }

        public static BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(DateSelector), defaultBindingMode: BindingMode.TwoWay);
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static BindableProperty DateProperty = BindableProperty.Create(nameof(Date), typeof(DateTime), typeof(DateSelector), defaultBindingMode: BindingMode.TwoWay, defaultValue: DateTime.Now);
        public DateTime Date
        {
            get => (DateTime)GetValue(DateProperty);
            set => SetValue(DateProperty, value);
        }

        public DateSelector()
        {
            InitializeComponent();
            DateControl.BindingContext = this;
            LabelControl.BindingContext = this;

            DateControl.Focused += (sender, e) =>
            {
                UpdateVisualState();
            };

            DateControl.Unfocused += (sender, e) =>
            {
                UpdateVisualState();
            };

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    UpdateVisualState();
                }
            };

            UpdateVisualState();
        }

        async void UpdateVisualState()
        {
            var tasks = new List<Task>();

            if (DateControl.IsFocused)
            {
                await UpdateFocusedVisualState();
            }
            else
            {
                await UpdateIdleVisualState();
            }
        }

        async Task UpdateIdleVisualState()
        {
            var tasks = new List<Task>();

            // color the bottom border
            var bottomBorderColor = IsEnabled ? _idleColor : _disabledColor;
            tasks.Add(BottomBorderControl.ColorTo(BottomBorderControl.Color, bottomBorderColor, c => BottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.ColorTo(ThickBottomBorderControl.Color, bottomBorderColor, c => ThickBottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            // color the text
            var textColor = IsEnabled ? _textColor : _disabledColor;
            tasks.Add(DateControl.ColorTo(DateControl.TextColor, textColor, c => DateControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // color the label
            var labelColor = IsEnabled ? _idleColor : _disabledColor;
            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, labelColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // color the image
            var imageOpacity = IsEnabled ? _idleImageOpacity : _disabledImageOpacity;
            tasks.Add(IconControl.FadeTo(imageOpacity, _animationLength, Easing.CubicInOut));

            // show the normal bottom border
            tasks.Add(BottomBorderControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            // hide the thick bottom border
            tasks.Add(ThickBottomBorderControl.FadeTo(0, _animationLength, Easing.CubicInOut));
      
            await Task.WhenAll(tasks);
        }

        //async Task UpdateIdleFilledVisualState()
        //{
        //    var tasks = new List<Task>();

        //    // color the bottom border
        //    var bottomBorderColor = IsEnabled ? BottomBorderIdleColor : BottomBorderDisabledColor;
        //    tasks.Add(BottomBorderControl.ColorTo(BottomBorderControl.Color, bottomBorderColor, c => BottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

        //    tasks.Add(ThickBottomBorderControl.ColorTo(ThickBottomBorderControl.Color, bottomBorderColor, c => ThickBottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

        //    // color the text
        //    var textColor = IsEnabled ? DateIdleColor : DateDisabledColor;
        //    tasks.Add(DateControl.ColorTo(DateControl.TextColor, textColor, c => DateControl.TextColor = c, _animationLength, Easing.CubicInOut));

        //    // color the label
        //    var labelColor = IsEnabled ? LabelIdleColor : LabelDisabledColor;
        //    tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, labelColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

        //    // show the normal bottom border
        //    tasks.Add(BottomBorderControl.FadeTo(1, _animationLength, Easing.CubicInOut));

        //    // hide the thick bottom border
        //    tasks.Add(ThickBottomBorderControl.FadeTo(0, _animationLength, Easing.CubicInOut));

        //    await Task.WhenAll(tasks);
        //}

        async Task UpdateFocusedVisualState()
        {
            var tasks = new List<Task>();

            tasks.Add(BottomBorderControl.ColorTo(BottomBorderControl.Color, _focusedColor, c => BottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.ColorTo(ThickBottomBorderControl.Color, _focusedColor, c => ThickBottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            // show the thick bottom border
            tasks.Add(ThickBottomBorderControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            // hide the normal bottom border
            tasks.Add(BottomBorderControl.FadeTo(0, _animationLength, Easing.CubicInOut));

            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, _focusedColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            tasks.Add(DateControl.ColorTo(DateControl.TextColor, _textColor, c => DateControl.TextColor = c, _animationLength, Easing.CubicInOut));

            tasks.Add(IconControl.FadeTo(_idleImageOpacity, _animationLength, Easing.CubicInOut));

            await Task.WhenAll(tasks);
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

