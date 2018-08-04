using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Forms.Animation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class ButtonSelector : AbsoluteLayout
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

        public static BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(ButtonSelector), defaultBindingMode: BindingMode.TwoWay);
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(ButtonSelector), defaultBindingMode: BindingMode.TwoWay);
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ButtonSelector), defaultBindingMode: BindingMode.TwoWay);
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ButtonSelector), defaultBindingMode: BindingMode.TwoWay);
        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public Dictionary<string, string> ReplaceColor
        {
            get
            {
                if (IsFocused)
                {
                    return new Dictionary<string, string> { { "currentColor", _focusedColor.GetHexString() } };
                }
                else if (IsEnabled)
                {
                    return new Dictionary<string, string> { { "currentColor", _idleColor.GetHexString() } };
                }
                else
                {
                    return new Dictionary<string, string> { { "currentColor", _disabledColor.GetHexString() } };
                }
            }
        }

        public ButtonSelector()
        {
            InitializeComponent();

            LabelControl.BindingContext = this;
            TextControl.BindingContext = this;
            BackgroundControl.BindingContext = this;
            IconControl.BindingContext = this;

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    UpdateVisualState();
                    BackgroundControl.IsEnabled = IsEnabled;
                    TextControl.IsEnabled = IsEnabled;
                }
            };

            TextControl.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(TextControl.Text))
                {
                    UpdateVisualState();
                }
            };

            Focused += (sender, e) =>
            {
                UpdateVisualState();
            };

            Unfocused += (sender, e) =>
            {
                UpdateVisualState();
            };

            UpdateVisualState();
        }

        async void UpdateVisualState()
        {
            if (IsFocused)
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

            OnPropertyChanged(nameof(ReplaceColor));
        }

        async Task SetIdleEmptyVisualState()
        {
            var tasks = new List<Task>();

            var disabled = ((Command != null && !Command.CanExecute(CommandParameter)) || !IsEnabled);

            // color the bottom border
            var bottomBorderColor = disabled ? _disabledColor : _idleColor;
            tasks.Add(BottomBorderControl.ColorTo(BottomBorderControl.Color, bottomBorderColor, c => BottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.ColorTo(ThickBottomBorderControl.Color, bottomBorderColor, c => ThickBottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            // color the text
            var textColor = disabled ? _disabledColor : _textColor;
            tasks.Add(TextControl.ColorTo(TextControl.TextColor, textColor, c => TextControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // color the label
            var labelColor = disabled ? _disabledColor : _idleColor;
            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, labelColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // show the normal bottom border
            tasks.Add(BottomBorderControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            // hide the thick bottom border
            tasks.Add(ThickBottomBorderControl.FadeTo(0, _animationLength, Easing.CubicInOut));

            tasks.Add(LabelControl.TranslateTo(0, 0, _animationLength, Easing.CubicInOut));

            tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, 16, f => LabelControl.FontSize = f, _animationLength, Easing.CubicInOut));

            await Task.WhenAll(tasks);
        }

        async Task SetIdleFilledVisualState()
        {
            var tasks = new List<Task>();

            var disabled = ((Command != null && !Command.CanExecute(CommandParameter)) || !IsEnabled);

            // color the bottom border
            var bottomBorderColor = disabled ? _disabledColor : _idleColor;
            tasks.Add(BottomBorderControl.ColorTo(BottomBorderControl.Color, bottomBorderColor, c => BottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.ColorTo(ThickBottomBorderControl.Color, bottomBorderColor, c => ThickBottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            // color the text
            var textColor = disabled ? _disabledColor : _textColor;
            tasks.Add(TextControl.ColorTo(TextControl.TextColor, textColor, c => TextControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // color the label
            var labelColor = disabled ? _disabledColor : _idleColor;
            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, labelColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // show the normal bottom border
            tasks.Add(BottomBorderControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            // hide the thick bottom border
            tasks.Add(ThickBottomBorderControl.FadeTo(0, _animationLength, Easing.CubicInOut));

            var translationY = Device.RuntimePlatform == Device.macOS ? 24 : -24;
            tasks.Add(LabelControl.TranslateTo(0, translationY, _animationLength, Easing.CubicInOut));

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
    }
}
