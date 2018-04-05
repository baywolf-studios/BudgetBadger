using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Forms.Animation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class DropdownSelector : AbsoluteLayout
    {
        uint _animationLength = 150;

        public static BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(DropdownSelector), defaultBindingMode: BindingMode.TwoWay);
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public BindingBase ItemDisplayBinding
        {
            get => PickerControl.ItemDisplayBinding;
            set => PickerControl.ItemDisplayBinding = value;
        }

        public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(DropdownSelector), -1, BindingMode.TwoWay);
        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(DropdownSelector), default(IList));
        public IList ItemsSource
        {
            get => (IList)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(DropdownSelector), null, BindingMode.TwoWay);
        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static BindableProperty BottomBorderIdleColorProperty = BindableProperty.Create(nameof(BottomBorderIdleColor), typeof(Color), typeof(DropdownSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.42));
        public Color BottomBorderIdleColor
        {
            get => (Color)GetValue(BottomBorderIdleColorProperty);
            set => SetValue(BottomBorderIdleColorProperty, value);
        }

        public static BindableProperty BottomBorderFocusedColorProperty = BindableProperty.Create(nameof(BottomBorderFocusedColor), typeof(Color), typeof(DropdownSelector), defaultValue: Color.Accent);
        public Color BottomBorderFocusedColor
        {
            get => (Color)GetValue(BottomBorderFocusedColorProperty);
            set => SetValue(BottomBorderFocusedColorProperty, value);
        }

        public static BindableProperty BottomBorderDisabledColorProperty = BindableProperty.Create(nameof(BottomBorderDisabledColor), typeof(Color), typeof(DropdownSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.42));
        public Color BottomBorderDisabledColor
        {
            get => (Color)GetValue(BottomBorderDisabledColorProperty);
            set => SetValue(BottomBorderDisabledColorProperty, value);
        }

        public static BindableProperty LabelIdleColorProperty = BindableProperty.Create(nameof(LabelIdleColor), typeof(Color), typeof(DropdownSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.54));
        public Color LabelIdleColor
        {
            get => (Color)GetValue(LabelIdleColorProperty);
            set => SetValue(LabelIdleColorProperty, value);
        }

        public static BindableProperty LabelFocusedColorProperty = BindableProperty.Create(nameof(LabelFocusedColor), typeof(Color), typeof(DropdownSelector), defaultValue: Color.FromRgba(Color.Accent.R, Color.Accent.G, Color.Accent.B, 0.87));
        public Color LabelFocusedColor
        {
            get => (Color)GetValue(LabelFocusedColorProperty);
            set => SetValue(LabelFocusedColorProperty, value);
        }

        public static BindableProperty LabelDisabledColorProperty = BindableProperty.Create(nameof(LabelDisabledColor), typeof(Color), typeof(DropdownSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.38));
        public Color LabelDisabledColor
        {
            get => (Color)GetValue(LabelDisabledColorProperty);
            set => SetValue(LabelDisabledColorProperty, value);
        }

        public static BindableProperty ItemIdleColorProperty = BindableProperty.Create(nameof(ItemIdleColor), typeof(Color), typeof(DropdownSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.87));
        public Color ItemIdleColor
        {
            get => (Color)GetValue(ItemIdleColorProperty);
            set => SetValue(ItemIdleColorProperty, value);
        }

        public static BindableProperty ItemFocusedColorProperty = BindableProperty.Create(nameof(ItemFocusedColor), typeof(Color), typeof(DropdownSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.87));
        public Color ItemFocusedColor
        {
            get => (Color)GetValue(ItemFocusedColorProperty);
            set => SetValue(ItemFocusedColorProperty, value);
        }

        public static BindableProperty ItemDisabledColorProperty = BindableProperty.Create(nameof(ItemDisabledColor), typeof(Color), typeof(DropdownSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.38));
        public Color ItemDisabledColor
        {
            get => (Color)GetValue(ItemDisabledColorProperty);
            set => SetValue(ItemDisabledColorProperty, value);
        }

        public static BindableProperty ErrorColorProperty = BindableProperty.Create(nameof(ErrorColor), typeof(Color), typeof(DropdownSelector), defaultValue: Color.Red);
        public Color ErrorColor
        {
            get => (Color)GetValue(ErrorColorProperty);
            set => SetValue(ErrorColorProperty, value);
        }

        public DropdownSelector()
        {
            InitializeComponent();
            PickerControl.BindingContext = this;
            LabelControl.BindingContext = this;

            PickerControl.Focused += (sender, e) =>
            {
                UpdateVisualState();
            };

            PickerControl.Unfocused += (sender, e) =>
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

            if (PickerControl.IsFocused)
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
            await PickerControl.ColorTo(PickerControl.TextColor, ItemIdleColor, c => PickerControl.TextColor = c, _animationLength, Easing.CubicInOut);
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
            await PickerControl.ColorTo(PickerControl.TextColor, ItemFocusedColor, c => PickerControl.TextColor = c, _animationLength, Easing.CubicInOut);
        }

        async Task SetDisabledColors()
        {
            var tasks = new List<Task>();

            tasks.Add(BottomBorderControl.ColorTo(BottomBorderControl.Color, BottomBorderDisabledColor, c => BottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.ColorTo(ThickBottomBorderControl.Color, BottomBorderDisabledColor, c => ThickBottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, LabelDisabledColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            tasks.Add(PickerControl.ColorTo(PickerControl.TextColor, ItemDisabledColor, c => PickerControl.TextColor = c, _animationLength, Easing.CubicInOut));

            await Task.WhenAll(tasks);
        }
    }
}

