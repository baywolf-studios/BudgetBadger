using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Forms.Animation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class MultilineTextEntry : AbsoluteLayout
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

        public MultilineTextEntry()
        {
            InitializeComponent();

            LabelControl.BindingContext = this;
            TextControl.BindingContext = this;
            //BindingContext = this;

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
                    TextControl.IsEnabled = IsEnabled;
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

            // color bottom border
            var bottomBorderColor = IsEnabled ? _idleColor : _disabledColor;
            tasks.Add(BottomBorderControl.ColorTo(BottomBorderControl.Color, bottomBorderColor, c => BottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.ColorTo(ThickBottomBorderControl.Color, bottomBorderColor, c => ThickBottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            // color text control
            var textColor = IsEnabled ? _textColor : _disabledColor;
            tasks.Add(TextControl.ColorTo(TextControl.TextColor, textColor, c => TextControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // color the label control
            var labelColor = IsEnabled ? _idleColor : _disabledColor;
            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, labelColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // reset bottom border
            tasks.Add(BottomBorderControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.FadeTo(0, _animationLength, Easing.CubicInOut));

            // reset position
            tasks.Add(LabelControl.TranslateTo(0, 0, _animationLength, Easing.CubicInOut));

            // reset font size to large
            tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, 16, f => LabelControl.FontSize = f, _animationLength, Easing.CubicInOut));

            await Task.WhenAll(tasks);
        }

        async Task SetIdleFilledVisualState()
        {
            var tasks = new List<Task>();

            // color bottom border
            var bottomBorderColor = IsEnabled ? _idleColor : _disabledColor;
            tasks.Add(BottomBorderControl.ColorTo(BottomBorderControl.Color, bottomBorderColor, c => BottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.ColorTo(ThickBottomBorderControl.Color, bottomBorderColor, c => ThickBottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            // color text control
            var textColor = IsEnabled ? _textColor : _disabledColor;
            tasks.Add(TextControl.ColorTo(TextControl.TextColor, textColor, c => TextControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // color the label control
            var labelColor = IsEnabled ? _idleColor : _disabledColor;
            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, labelColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // show the bottom border
            tasks.Add(BottomBorderControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.FadeTo(0, _animationLength, Easing.CubicInOut));

            // move the label upward
            var translationY = Device.RuntimePlatform == Device.macOS ? 24 : -24;
            tasks.Add(LabelControl.TranslateTo(0, translationY, _animationLength, Easing.CubicInOut));

            // shrink the label
            tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, 12, f => LabelControl.FontSize = f, _animationLength, Easing.CubicInOut));

            await Task.WhenAll(tasks);
        }

        async Task SetFocusedVisualState()
        {
            var tasks = new List<Task>();

            tasks.Add(BottomBorderControl.ColorTo(BottomBorderControl.Color, _focusedColor, c => BottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.ColorTo(ThickBottomBorderControl.Color, _focusedColor, c => ThickBottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            // show the thick bottom border
            tasks.Add(ThickBottomBorderControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            // hide the normal bottom border
            tasks.Add(BottomBorderControl.FadeTo(0, _animationLength, Easing.CubicInOut));

            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, _focusedColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            var translationY = Device.RuntimePlatform == Device.macOS ? 24 : -24;
            tasks.Add(LabelControl.TranslateTo(0, translationY, _animationLength, Easing.CubicInOut));

            tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, 12, f => LabelControl.FontSize = f, _animationLength, Easing.CubicInOut));

            tasks.Add(TextControl.ColorTo(TextControl.TextColor, _textColor, c => TextControl.TextColor = c, _animationLength, Easing.CubicInOut));

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
