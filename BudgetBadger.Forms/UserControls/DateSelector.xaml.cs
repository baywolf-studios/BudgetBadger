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

        public static BindableProperty BottomBorderIdleColorProperty = BindableProperty.Create(nameof(BottomBorderIdleColor), typeof(Color), typeof(DateSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.42));
        public Color BottomBorderIdleColor
        {
            get => (Color)GetValue(BottomBorderIdleColorProperty);
            set => SetValue(BottomBorderIdleColorProperty, value);
        }

        public static BindableProperty BottomBorderFocusedColorProperty = BindableProperty.Create(nameof(BottomBorderFocusedColor), typeof(Color), typeof(DateSelector), defaultValue: Color.Accent);
        public Color BottomBorderFocusedColor
        {
            get => (Color)GetValue(BottomBorderFocusedColorProperty);
            set => SetValue(BottomBorderFocusedColorProperty, value);
        }

        public static BindableProperty BottomBorderDisabledColorProperty = BindableProperty.Create(nameof(BottomBorderDisabledColor), typeof(Color), typeof(DateSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.42));
        public Color BottomBorderDisabledColor
        {
            get => (Color)GetValue(BottomBorderDisabledColorProperty);
            set => SetValue(BottomBorderDisabledColorProperty, value);
        }

        public static BindableProperty LabelIdleColorProperty = BindableProperty.Create(nameof(LabelIdleColor), typeof(Color), typeof(DateSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.54));
        public Color LabelIdleColor
        {
            get => (Color)GetValue(LabelIdleColorProperty);
            set => SetValue(LabelIdleColorProperty, value);
        }

        public static BindableProperty LabelFocusedColorProperty = BindableProperty.Create(nameof(LabelFocusedColor), typeof(Color), typeof(DateSelector), defaultValue: Color.FromRgba(Color.Accent.R, Color.Accent.G, Color.Accent.B, 0.87));
        public Color LabelFocusedColor
        {
            get => (Color)GetValue(LabelFocusedColorProperty);
            set => SetValue(LabelFocusedColorProperty, value);
        }

        public static BindableProperty LabelDisabledColorProperty = BindableProperty.Create(nameof(LabelDisabledColor), typeof(Color), typeof(DateSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.38));
        public Color LabelDisabledColor
        {
            get => (Color)GetValue(LabelDisabledColorProperty);
            set => SetValue(LabelDisabledColorProperty, value);
        }

        public static BindableProperty DateIdleColorProperty = BindableProperty.Create(nameof(DateIdleColor), typeof(Color), typeof(DateSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.87));
        public Color DateIdleColor
        {
            get => (Color)GetValue(DateIdleColorProperty);
            set => SetValue(DateIdleColorProperty, value);
        }

        public static BindableProperty DateFocusedColorProperty = BindableProperty.Create(nameof(DateFocusedColor), typeof(Color), typeof(DateSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.87));
        public Color DateFocusedColor
        {
            get => (Color)GetValue(DateFocusedColorProperty);
            set => SetValue(DateFocusedColorProperty, value);
        }

        public static BindableProperty DateDisabledColorProperty = BindableProperty.Create(nameof(DateDisabledColor), typeof(Color), typeof(DateSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.38));
        public Color DateDisabledColor
        {
            get => (Color)GetValue(DateDisabledColorProperty);
            set => SetValue(DateDisabledColorProperty, value);
        }

        public static BindableProperty ErrorColorProperty = BindableProperty.Create(nameof(ErrorColor), typeof(Color), typeof(DateSelector), defaultValue: Color.Red);
        public Color ErrorColor
        {
            get => (Color)GetValue(ErrorColorProperty);
            set => SetValue(ErrorColorProperty, value);
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
                tasks.Add(SetBottomBorderFocused());
                tasks.Add(SetLabelFocused());
                tasks.Add(SetDateFocused());
            }
            else
            {
                tasks.Add(SetBottomBorderIdle());
                tasks.Add(SetLabelIdle());
                tasks.Add(SetDateIdle());
            }

            await Task.WhenAll(tasks);

            if (!IsEnabled)
            {
                await SetDisabledColors();
            }
        }

        async Task SetBottomBorderIdle()
        {
            var tasks = new List<Task>();

            // color the bottom border
            tasks.Add(BottomBorderControl.ColorTo(BottomBorderControl.Color, BottomBorderIdleColor, c => BottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.ColorTo(ThickBottomBorderControl.Color, BottomBorderIdleColor, c => ThickBottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            // show the normal bottom border
            tasks.Add(BottomBorderControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            // hide the thick bottom border
            tasks.Add(ThickBottomBorderControl.FadeTo(0, _animationLength, Easing.CubicInOut));

            await Task.WhenAll(tasks);
        }

        async Task SetLabelIdle()
        {
            await LabelControl.ColorTo(LabelControl.TextColor, LabelIdleColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut);
        }

        async Task SetDateIdle()
        {
            await DateControl.ColorTo(DateControl.TextColor, DateIdleColor, c => DateControl.TextColor = c, _animationLength, Easing.CubicInOut);
        }

        async Task SetBottomBorderFocused()
        {
            var tasks = new List<Task>();

            tasks.Add(BottomBorderControl.ColorTo(BottomBorderControl.Color, BottomBorderFocusedColor, c => BottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.ColorTo(ThickBottomBorderControl.Color, BottomBorderFocusedColor, c => ThickBottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            // show the thick bottom border
            tasks.Add(ThickBottomBorderControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            // hide the normal bottom border
            tasks.Add(BottomBorderControl.FadeTo(0, _animationLength, Easing.CubicInOut));

            await Task.WhenAll(tasks);
        }

        async Task SetLabelFocused()
        {
            await LabelControl.ColorTo(LabelControl.TextColor, LabelFocusedColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut);
        }

        async Task SetDateFocused()
        {
            await DateControl.ColorTo(DateControl.TextColor, DateFocusedColor, c => DateControl.TextColor = c, _animationLength, Easing.CubicInOut);
        }

        async Task SetDisabledColors()
        {
            var tasks = new List<Task>();

            tasks.Add(BottomBorderControl.ColorTo(BottomBorderControl.Color, BottomBorderDisabledColor, c => BottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.ColorTo(ThickBottomBorderControl.Color, BottomBorderDisabledColor, c => ThickBottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, LabelDisabledColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            tasks.Add(DateControl.ColorTo(DateControl.TextColor, DateDisabledColor, c => DateControl.TextColor = c, _animationLength, Easing.CubicInOut));

            await Task.WhenAll(tasks);
        }
    }
}

