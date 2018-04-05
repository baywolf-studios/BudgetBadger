﻿using System;
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

        public static BindableProperty BottomBorderIdleColorProperty = BindableProperty.Create(nameof(BottomBorderIdleColor), typeof(Color), typeof(ButtonSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.42));
        public Color BottomBorderIdleColor
        {
            get => (Color)GetValue(BottomBorderIdleColorProperty);
            set => SetValue(BottomBorderIdleColorProperty, value);
        }

        public static BindableProperty BottomBorderFocusedColorProperty = BindableProperty.Create(nameof(BottomBorderFocusedColor), typeof(Color), typeof(ButtonSelector), defaultValue: Color.Accent);
        public Color BottomBorderFocusedColor
        {
            get => (Color)GetValue(BottomBorderFocusedColorProperty);
            set => SetValue(BottomBorderFocusedColorProperty, value);
        }

        public static BindableProperty BottomBorderDisabledColorProperty = BindableProperty.Create(nameof(BottomBorderDisabledColor), typeof(Color), typeof(ButtonSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.42));
        public Color BottomBorderDisabledColor
        {
            get => (Color)GetValue(BottomBorderDisabledColorProperty);
            set => SetValue(BottomBorderDisabledColorProperty, value);
        }

        public static BindableProperty LabelIdleColorProperty = BindableProperty.Create(nameof(LabelIdleColor), typeof(Color), typeof(ButtonSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.54));
        public Color LabelIdleColor
        {
            get => (Color)GetValue(LabelIdleColorProperty);
            set => SetValue(LabelIdleColorProperty, value);
        }

        public static BindableProperty LabelFocusedColorProperty = BindableProperty.Create(nameof(LabelFocusedColor), typeof(Color), typeof(ButtonSelector), defaultValue: Color.FromRgba(Color.Accent.R, Color.Accent.G, Color.Accent.B, 0.87));
        public Color LabelFocusedColor
        {
            get => (Color)GetValue(LabelFocusedColorProperty);
            set => SetValue(LabelFocusedColorProperty, value);
        }

        public static BindableProperty LabelDisabledColorProperty = BindableProperty.Create(nameof(LabelDisabledColor), typeof(Color), typeof(ButtonSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.38));
        public Color LabelDisabledColor
        {
            get => (Color)GetValue(LabelDisabledColorProperty);
            set => SetValue(LabelDisabledColorProperty, value);
        }

        public static BindableProperty TextIdleColorProperty = BindableProperty.Create(nameof(TextIdleColor), typeof(Color), typeof(ButtonSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.87));
        public Color TextIdleColor
        {
            get => (Color)GetValue(TextIdleColorProperty);
            set => SetValue(TextIdleColorProperty, value);
        }

        public static BindableProperty TextFocusedColorProperty = BindableProperty.Create(nameof(TextFocusedColor), typeof(Color), typeof(ButtonSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.87));
        public Color TextFocusedColor
        {
            get => (Color)GetValue(TextFocusedColorProperty);
            set => SetValue(TextFocusedColorProperty, value);
        }

        public static BindableProperty TextDisabledColorProperty = BindableProperty.Create(nameof(TextDisabledColor), typeof(Color), typeof(ButtonSelector), defaultValue: Color.FromRgba(0, 0, 0, 0.38));
        public Color TextDisabledColor
        {
            get => (Color)GetValue(TextDisabledColorProperty);
            set => SetValue(TextDisabledColorProperty, value);
        }

        public static BindableProperty ErrorColorProperty = BindableProperty.Create(nameof(ErrorColor), typeof(Color), typeof(ButtonSelector), defaultValue: Color.Red);
        public Color ErrorColor
        {
            get => (Color)GetValue(ErrorColorProperty);
            set => SetValue(ErrorColorProperty, value);
        }

        public ButtonSelector()
        {
            InitializeComponent();

            LabelControl.BindingContext = this;
            TextControl.BindingContext = this;

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Command = Command;
            tapGestureRecognizer.CommandParameter = CommandParameter;
            GestureRecognizers.Add(tapGestureRecognizer);

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    UpdateVisualState();
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
            var tasks = new List<Task>();

            if (IsFocused)
            {
                tasks.Add(SetBottomBorderFocused());
                tasks.Add(SetLabelFocused());
                tasks.Add(SetTextFocused());
            }
            else if (string.IsNullOrEmpty(Text))
            {
                tasks.Add(SetBottomBorderIdle());
                tasks.Add(SetLabelIdleEmpty());
                tasks.Add(SetTextIdle());
            }
            else
            {
                tasks.Add(SetBottomBorderIdle());
                tasks.Add(SetLabelIdleFilled());
                tasks.Add(SetTextIdle());
            }

            await Task.WhenAll(tasks);

            if ((Command != null && !Command.CanExecute(CommandParameter)) || !IsEnabled)
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

        async Task SetLabelIdleEmpty()
        {
            var tasks = new List<Task>();

            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, LabelIdleColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            tasks.Add(LabelControl.TranslateTo(0, 0, _animationLength, Easing.CubicInOut));

            tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, 16, f => LabelControl.FontSize = f, _animationLength, Easing.CubicInOut));

            await Task.WhenAll(tasks);
        }

        async Task SetLabelIdleFilled()
        {
            var tasks = new List<Task>();

            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, LabelIdleColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            var translationY = Device.RuntimePlatform == Device.macOS ? 24 : -24;
            tasks.Add(LabelControl.TranslateTo(0, translationY, _animationLength, Easing.CubicInOut));

            tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, 12, f => LabelControl.FontSize = f, _animationLength, Easing.CubicInOut));

            await Task.WhenAll(tasks);
        }

        async Task SetTextIdle()
        {
            await TextControl.ColorTo(TextControl.TextColor, TextIdleColor, c => TextControl.TextColor = c, _animationLength, Easing.CubicInOut);
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
            var tasks = new List<Task>();
            
            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, LabelFocusedColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            var translationY = Device.RuntimePlatform == Device.macOS ? 24 : -24;
            tasks.Add(LabelControl.TranslateTo(0, translationY, _animationLength, Easing.CubicInOut));

            tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, 12, f => LabelControl.FontSize = f, _animationLength, Easing.CubicInOut));

            await Task.WhenAll(tasks);
        }

        async Task SetTextFocused()
        {
            await TextControl.ColorTo(TextControl.TextColor, TextFocusedColor, c => TextControl.TextColor = c, _animationLength, Easing.CubicInOut);
        }

        async Task SetDisabledColors()
        {
            var tasks = new List<Task>();

            tasks.Add(BottomBorderControl.ColorTo(BottomBorderControl.Color, BottomBorderDisabledColor, c => BottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.ColorTo(ThickBottomBorderControl.Color, BottomBorderDisabledColor, c => ThickBottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, LabelDisabledColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            tasks.Add(TextControl.ColorTo(TextControl.TextColor, TextDisabledColor, c => TextControl.TextColor = c, _animationLength, Easing.CubicInOut));

            await Task.WhenAll(tasks);
        }
    }
}
