using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using BudgetBadger.Forms.Animation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class TextEntry : AbsoluteLayout
    {
        uint _animationLength = 150;

        public static BindableProperty PrefixProperty = BindableProperty.Create(nameof(Prefix), typeof(string), typeof(TextEntry), defaultBindingMode: BindingMode.TwoWay);
        public string Prefix
        {
            get => (string)GetValue(PrefixProperty);
            set => SetValue(PrefixProperty, value);
        }

        public static BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(TextEntry), defaultBindingMode: BindingMode.TwoWay);
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(TextEntry), defaultBindingMode: BindingMode.TwoWay);
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static BindableProperty IsPasswordProperty = BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(TextEntry), defaultValue: false);
        public bool IsPassword
        {
            get => (bool)GetValue(IsPasswordProperty);
            set => SetValue(IsPasswordProperty, value);
        }

        public static BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(TextEntry), defaultValue: Keyboard.Default);
        public Keyboard Keyboard
        {
            get => (Keyboard)GetValue(KeyboardProperty);
            set => SetValue(KeyboardProperty, value);
        }

        public static BindableProperty BottomBorderIdleColorProperty = BindableProperty.Create(nameof(BottomBorderIdleColor), typeof(Color), typeof(TextEntry), defaultValue: Color.FromRgba(0, 0, 0, 0.42));
        public Color BottomBorderIdleColor
        {
            get => (Color)GetValue(BottomBorderIdleColorProperty);
            set => SetValue(BottomBorderIdleColorProperty, value);
        }

        public static BindableProperty BottomBorderFocusedColorProperty = BindableProperty.Create(nameof(BottomBorderFocusedColor), typeof(Color), typeof(TextEntry), defaultValue: Color.Accent);
        public Color BottomBorderFocusedColor
        {
            get => (Color)GetValue(BottomBorderFocusedColorProperty);
            set => SetValue(BottomBorderFocusedColorProperty, value);
        }

        public static BindableProperty BottomBorderDisabledColorProperty = BindableProperty.Create(nameof(BottomBorderDisabledColor), typeof(Color), typeof(TextEntry), defaultValue: Color.FromRgba(0, 0, 0, 0.42));
        public Color BottomBorderDisabledColor
        {
            get => (Color)GetValue(BottomBorderDisabledColorProperty);
            set => SetValue(BottomBorderDisabledColorProperty, value);
        }

        public static BindableProperty LabelIdleColorProperty = BindableProperty.Create(nameof(LabelIdleColor), typeof(Color), typeof(TextEntry), defaultValue: Color.FromRgba(0, 0, 0, 0.54));
        public Color LabelIdleColor
        {
            get => (Color)GetValue(LabelIdleColorProperty);
            set => SetValue(LabelIdleColorProperty, value);
        }

        public static BindableProperty LabelFocusedColorProperty = BindableProperty.Create(nameof(LabelFocusedColor), typeof(Color), typeof(TextEntry), defaultValue: Color.FromRgba(Color.Accent.R, Color.Accent.G, Color.Accent.B, 0.87));
        public Color LabelFocusedColor
        {
            get => (Color)GetValue(LabelFocusedColorProperty);
            set => SetValue(LabelFocusedColorProperty, value);
        }

        public static BindableProperty LabelDisabledColorProperty = BindableProperty.Create(nameof(LabelDisabledColor), typeof(Color), typeof(TextEntry), defaultValue: Color.FromRgba(0, 0, 0, 0.38));
        public Color LabelDisabledColor
        {
            get => (Color)GetValue(LabelDisabledColorProperty);
            set => SetValue(LabelDisabledColorProperty, value);
        }

        public static BindableProperty TextIdleColorProperty = BindableProperty.Create(nameof(TextIdleColor), typeof(Color), typeof(TextEntry), defaultValue: Color.FromRgba(0, 0, 0, 0.87));
        public Color TextIdleColor
        {
            get => (Color)GetValue(TextIdleColorProperty);
            set => SetValue(TextIdleColorProperty, value);
        }

        public static BindableProperty TextFocusedColorProperty = BindableProperty.Create(nameof(TextFocusedColor), typeof(Color), typeof(TextEntry), defaultValue: Color.FromRgba(0, 0, 0, 0.87));
        public Color TextFocusedColor
        {
            get => (Color)GetValue(TextFocusedColorProperty);
            set => SetValue(TextFocusedColorProperty, value);
        }

        public static BindableProperty TextDisabledColorProperty = BindableProperty.Create(nameof(TextDisabledColor), typeof(Color), typeof(TextEntry), defaultValue: Color.FromRgba(0, 0, 0, 0.38));
        public Color TextDisabledColor
        {
            get => (Color)GetValue(TextDisabledColorProperty);
            set => SetValue(TextDisabledColorProperty, value);
        }

        public static BindableProperty ErrorColorProperty = BindableProperty.Create(nameof(ErrorColor), typeof(Color), typeof(TextEntry), defaultValue: Color.Red);
        public Color ErrorColor
        {
            get => (Color)GetValue(ErrorColorProperty);
            set => SetValue(ErrorColorProperty, value);
        }

        public static BindableProperty PrefixColorProperty = BindableProperty.Create(nameof(PrefixColor), typeof(Color), typeof(TextEntry), defaultValue: Color.FromRgba(0, 0, 0, 0.54));
        public Color PrefixColor
        {
            get => (Color)GetValue(PrefixColorProperty);
            set => SetValue(PrefixColorProperty, value);
        }

        public static BindableProperty PrefixDisabledColorProperty = BindableProperty.Create(nameof(PrefixDisabledColor), typeof(Color), typeof(TextEntry), defaultValue: Color.FromRgba(0, 0, 0, 0.38));
        public Color PrefixDisabledColor
        {
            get => (Color)GetValue(PrefixDisabledColorProperty);
            set => SetValue(PrefixDisabledColorProperty, value);
        }

        public TextEntry()
        {
            InitializeComponent();

            PrefixControl.BindingContext = this;
            LabelControl.BindingContext = this;
            TextControl.BindingContext = this;

            TextControl.Focused += (sender, e) =>
            {
                //EntryFocused?.Invoke(this, e);
                UpdateVisualState();
            };

            TextControl.Unfocused += (sender, e) =>
            {
                //EntryUnfocused?.Invoke(this, e);
                UpdateVisualState();
            };

            TextControl.TextChanged += (sender, e) =>
            {
                //TextChanged?.Invoke(sender, e);
                UpdateVisualState();
            };

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    UpdateVisualState();
                }
            };

            if (Device.RuntimePlatform == Device.macOS)
            {
                TextControl.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(TextControl.IsFocused))
                    {
                        UpdateVisualState();
                    }
                };

                TextControl.Completed += (sender, e) =>
                {
                    UpdateVisualState();
                };
            }

            UpdateVisualState();
        }

        async void UpdateVisualState()
        {
            if (TextControl.IsFocused)
            {
                await SetFocusedVisualState();
            }
            else if (string.IsNullOrEmpty(Text))
            {
                await SetIdleEmptyVisualState();
            }
            else
            {
                await SetIdleFilledVisualState();
            }
        }

        async Task SetIdleEmptyVisualState()
        {
            var tasks = new List<Task>();

            // color the bottom border
            var bottomBorderColor = IsEnabled ? BottomBorderIdleColor : BottomBorderDisabledColor;
            tasks.Add(BottomBorderControl.ColorTo(BottomBorderControl.Color, bottomBorderColor, c => BottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.ColorTo(ThickBottomBorderControl.Color, bottomBorderColor, c => ThickBottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            // color the text
            var textColor = IsEnabled ? TextIdleColor : TextDisabledColor;
            tasks.Add(TextControl.ColorTo(TextControl.TextColor, textColor, c => TextControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // color the label
            var labelColor = IsEnabled ? LabelIdleColor : LabelDisabledColor;
            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, labelColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // show the normal bottom border
            tasks.Add(BottomBorderControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            // hide the thick bottom border
            tasks.Add(ThickBottomBorderControl.FadeTo(0, _animationLength, Easing.CubicInOut));

            // reset to original position
            tasks.Add(LabelControl.TranslateTo(0, 0, _animationLength, Easing.CubicInOut));

            // reset to original font size
            tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, 16, f => LabelControl.FontSize = f, _animationLength, Easing.CubicInOut));

            // hide the prefix
            tasks.Add(PrefixControl.FadeTo(0, _animationLength, Easing.CubicInOut));

            await Task.WhenAll(tasks);
        }

        async Task SetIdleFilledVisualState()
        {
            var tasks = new List<Task>();

            // color the bottom border
            var bottomBorderColor = IsEnabled ? BottomBorderIdleColor : BottomBorderDisabledColor;
            tasks.Add(BottomBorderControl.ColorTo(BottomBorderControl.Color, bottomBorderColor, c => BottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.ColorTo(ThickBottomBorderControl.Color, bottomBorderColor, c => ThickBottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            // color the text
            var textColor = IsEnabled ? TextIdleColor : TextDisabledColor;
            tasks.Add(TextControl.ColorTo(TextControl.TextColor, textColor, c => TextControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // color the label
            var labelColor = IsEnabled ? LabelIdleColor : LabelDisabledColor;
            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, labelColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // color the prefix
            var prefixColor = IsEnabled ? PrefixColor : PrefixDisabledColor;
            tasks.Add(PrefixControl.ColorTo(PrefixControl.TextColor, prefixColor, c => PrefixControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // show the normal bottom border
            tasks.Add(BottomBorderControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            // hide the thick bottom border
            tasks.Add(ThickBottomBorderControl.FadeTo(0, _animationLength, Easing.CubicInOut));

            // move label upward
            var translationY = Device.RuntimePlatform == Device.macOS ? 24 : -24;
            tasks.Add(LabelControl.TranslateTo(0, translationY, _animationLength, Easing.CubicInOut));

            // shrink label text
            tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, 12, f => LabelControl.FontSize = f, _animationLength, Easing.CubicInOut));

            // show the prefix
            tasks.Add(PrefixControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            //move entry over
            var entryLeftMargin = string.IsNullOrEmpty(Prefix) ? 0 : PrefixControl.Width + 4;
            var entryMargin = new Thickness(entryLeftMargin, 0, 0, 0);
            tasks.Add(PrefixControl.ThicknessTo(PrefixControl.Margin, entryMargin, m => PrefixControl.Margin = m, _animationLength, Easing.CubicInOut));

            await Task.WhenAll(tasks);
        }

        async Task SetFocusedVisualState()
        {
            var tasks = new List<Task>();

            // color bottom border
            tasks.Add(BottomBorderControl.ColorTo(BottomBorderControl.Color, BottomBorderFocusedColor, c => BottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.ColorTo(ThickBottomBorderControl.Color, BottomBorderFocusedColor, c => ThickBottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            // color the label control
            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, LabelFocusedColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // color the text control
            tasks.Add(TextControl.ColorTo(TextControl.TextColor, TextFocusedColor, c => TextControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // color the prefix
            tasks.Add(PrefixControl.ColorTo(PrefixControl.TextColor, PrefixColor, c => PrefixControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // show the thick bottom border
            tasks.Add(ThickBottomBorderControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            // hide the normal bottom border
            tasks.Add(BottomBorderControl.FadeTo(0, _animationLength, Easing.CubicInOut));

            // move upward
            var translationY = Device.RuntimePlatform == Device.macOS ? 24 : -24;
            tasks.Add(LabelControl.TranslateTo(0, translationY, _animationLength, Easing.CubicInOut));

            // shrink font size
            tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, 12, f => LabelControl.FontSize = f, _animationLength, Easing.CubicInOut));

            // show the prefix
            tasks.Add(PrefixControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            //move entry over
            var entryLeftMargin = string.IsNullOrEmpty(Prefix) ? 0 : PrefixControl.Width + 4;
            var entryMargin = new Thickness(entryLeftMargin, 0, 0, 0);
            tasks.Add(TextControl.ThicknessTo(TextControl.Margin, entryMargin, m => TextControl.Margin = m, _animationLength, Easing.CubicInOut));

            await Task.WhenAll(tasks);
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
