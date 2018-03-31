using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetBadger.Forms.Animation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class ButtonSelector : ContentView
    {
        uint _animationLength = 150;

        Color PrimaryColor38
        {
            get => Color.FromRgba(PrimaryColor.R, PrimaryColor.G, PrimaryColor.B, 0.38);
        }

        Color PrimaryColor42 
        {
            get => Color.FromRgba(PrimaryColor.R, PrimaryColor.G, PrimaryColor.B, 0.42);
        }

        Color PrimaryColor54
        {
            get => Color.FromRgba(PrimaryColor.R, PrimaryColor.G, PrimaryColor.B, 0.54);
        }

        Color PrimaryColor87
        {
            get => Color.FromRgba(PrimaryColor.R, PrimaryColor.G, PrimaryColor.B, 0.87);
        }

        Color AccentColor87
        {
            get => Color.FromRgba(AccentColor.R, AccentColor.G, AccentColor.B, 0.87);
        }

        public static void Init() { }

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

        public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(ButtonSelector), defaultBindingMode: BindingMode.TwoWay);
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(ButtonSelector), defaultBindingMode: BindingMode.TwoWay);
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static BindableProperty PrimaryColorProperty = BindableProperty.Create(nameof(PrimaryColor), typeof(Color), typeof(ButtonSelector), defaultValue: Color.Black);
        public Color PrimaryColor
        {
            get => (Color)GetValue(PrimaryColorProperty);
            set => SetValue(PrimaryColorProperty, value);
        }

        public static BindableProperty AccentColorProperty = BindableProperty.Create(nameof(AccentColor), typeof(Color), typeof(ButtonSelector), defaultValue: Color.Accent);
        public Color AccentColor
        {
            get => (Color)GetValue(AccentColorProperty);
            set => SetValue(AccentColorProperty, value);
        }

        public ButtonSelector()
        {
            InitializeComponent();

            LabelControl.BindingContext = this;
            TextControl.BindingContext = this;
            GridControl.BindingContext = this;


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
            if (TextControl.IsFocused)
            {
                await SetFocusedState();
            }
            else if (string.IsNullOrEmpty(TextControl.Text))
            {
                await SetIdleEmptyState();
            }
            else if (!string.IsNullOrEmpty(TextControl.Text))
            {
                await SetIdleFilledState();
            }

            if ((Command != null && !Command.CanExecute(CommandParameter)) || !IsEnabled)
            {
                await UpdateDisabled();
            }
        }

        async Task UpdateDisabled()
        {
            var tasks = new List<Task>();

            if (BottomBorder.BackgroundColor != PrimaryColor42)
            {
                tasks.Add(BottomBorder.ColorTo(BottomBorder.BackgroundColor, PrimaryColor42, c => BottomBorder.BackgroundColor = c, _animationLength, Easing.CubicIn));
            }

            if (ThickBottomBorder.BackgroundColor != PrimaryColor42)
            {
                tasks.Add(ThickBottomBorder.ColorTo(ThickBottomBorder.BackgroundColor, PrimaryColor42, c => ThickBottomBorder.BackgroundColor = c, _animationLength, Easing.CubicIn));
            }

            if (LabelControl.TextColor != PrimaryColor38)
            {
                tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, PrimaryColor38, c => LabelControl.TextColor = c, _animationLength, Easing.CubicIn));
            }

            if (TextControl.TextColor != PrimaryColor38)
            {
                tasks.Add(TextControl.ColorTo(TextControl.TextColor, PrimaryColor38, c => TextControl.TextColor = c, _animationLength, Easing.CubicIn));
            }

            await Task.WhenAll(tasks);
        }

        async Task SetIdleEmptyState()
        {
            var tasks = new List<Task>();

            // color the main text
            if (TextControl.TextColor != PrimaryColor87)
            {
                tasks.Add(TextControl.ColorTo(TextControl.TextColor, PrimaryColor87, c => TextControl.TextColor = c, _animationLength, Easing.CubicIn));
            }

            // color the bottom borders
            if (BottomBorder.BackgroundColor != PrimaryColor42)
            {
                tasks.Add(BottomBorder.ColorTo(BottomBorder.BackgroundColor, PrimaryColor42, c => BottomBorder.BackgroundColor = c, _animationLength, Easing.CubicIn));
            }

            if (ThickBottomBorder.BackgroundColor != PrimaryColor42)
            {
                tasks.Add(ThickBottomBorder.ColorTo(ThickBottomBorder.BackgroundColor, PrimaryColor42, c => ThickBottomBorder.BackgroundColor = c, _animationLength, Easing.CubicIn));
            }

            // show the normal bottom border
            if (BottomBorder.Opacity < 1)
            {
                tasks.Add(BottomBorder.FadeTo(1, _animationLength, Easing.CubicIn));
            }

            // hide the thick bottom border
            if (ThickBottomBorder.Opacity > 0)
            {
                tasks.Add(ThickBottomBorder.FadeTo(0, _animationLength, Easing.CubicOut));
            }

            // reset the label position
            if (LabelControl.TranslationX != 0 || LabelControl.TranslationY != 0)
            {
                tasks.Add(LabelControl.TranslateTo(0, 0, _animationLength, Easing.CubicIn));
            }

            // color the label
            if (LabelControl.TextColor != PrimaryColor54)
            {
                tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, PrimaryColor54, c => LabelControl.TextColor = c, _animationLength, Easing.CubicIn));
            }

            // increase the font size of the label
            if (LabelControl.FontSize != TextControl.FontSize)
            {
                tasks.Add(LabelControl.FontSizeTo(LabelControl.FontSize, TextControl.FontSize, f => LabelControl.FontSize = f, _animationLength, Easing.CubicIn));
            }

            await Task.WhenAll(tasks);
        }

        async Task SetIdleFilledState()
        {
            var tasks = new List<Task>();

            // color the main text
            if (TextControl.TextColor != PrimaryColor87)
            {
                tasks.Add(TextControl.ColorTo(TextControl.TextColor, PrimaryColor87, c => TextControl.TextColor = c, _animationLength, Easing.CubicIn));
            }

            // color the bottom borders
            if (BottomBorder.BackgroundColor != PrimaryColor42)
            {
                tasks.Add(BottomBorder.ColorTo(BottomBorder.BackgroundColor, PrimaryColor42, c => BottomBorder.BackgroundColor = c, _animationLength, Easing.CubicIn));
            }

            if (ThickBottomBorder.BackgroundColor != PrimaryColor42)
            {
                tasks.Add(ThickBottomBorder.ColorTo(ThickBottomBorder.BackgroundColor, PrimaryColor42, c => ThickBottomBorder.BackgroundColor = c, _animationLength, Easing.CubicOut));
            }

            // show the normal bottom border
            if (BottomBorder.Opacity < 1)
            {
                tasks.Add(BottomBorder.FadeTo(1, _animationLength, Easing.CubicIn));
            }

            // hide the thick bottom border
            if (ThickBottomBorder.Opacity > 0)
            {
                tasks.Add(ThickBottomBorder.FadeTo(0, _animationLength, Easing.CubicOut));
            }

            // move the label above the editor
            var labelTranslateX = HiddenLabelControl.X - TextControl.X;
            var labelTranslateY = HiddenLabelControl.Y - TextControl.Y;
            if (Device.RuntimePlatform == Device.macOS)
            {
                labelTranslateX = -1 * labelTranslateX;
                labelTranslateY = -1 * labelTranslateY;
            }
            if (LabelControl.TranslationX != labelTranslateX || LabelControl.TranslationY != labelTranslateY)
            {
                tasks.Add(LabelControl.TranslateTo(labelTranslateX, labelTranslateY, _animationLength, Easing.CubicIn));
            }

            // color the label control
            if (LabelControl.TextColor != PrimaryColor54)
            {
                tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, PrimaryColor54, c => LabelControl.TextColor = c, _animationLength, Easing.CubicIn));
            }

            // shrink the font size down
            if (LabelControl.FontSize != HiddenLabelControl.FontSize)
            {
                tasks.Add(LabelControl.FontSizeTo(LabelControl.FontSize, HiddenLabelControl.FontSize, f => LabelControl.FontSize = f, _animationLength, Easing.CubicIn));
            }

            await Task.WhenAll(tasks);
        }

        async Task SetFocusedState()
        {
            var tasks = new List<Task>();

            // set the text color
            if (TextControl.TextColor != PrimaryColor87)
            {
                tasks.Add(TextControl.ColorTo(TextControl.TextColor, PrimaryColor87, c => TextControl.TextColor = c, _animationLength, Easing.CubicIn));
            }

            // color the bottom borders
            if (BottomBorder.BackgroundColor != AccentColor)
            {
                tasks.Add(BottomBorder.ColorTo(BottomBorder.BackgroundColor, AccentColor, c => BottomBorder.BackgroundColor = c, _animationLength, Easing.CubicIn));
            }

            if (ThickBottomBorder.BackgroundColor != AccentColor)
            {
                tasks.Add(ThickBottomBorder.ColorTo(ThickBottomBorder.BackgroundColor, AccentColor, c => ThickBottomBorder.BackgroundColor = c, _animationLength, Easing.CubicOut));
            }

            // hide the normal bottom border
            if (BottomBorder.Opacity > 0)
            {
                tasks.Add(BottomBorder.FadeTo(0, _animationLength, Easing.CubicOut));
            }

            // show the thick bottom border
            if (ThickBottomBorder.Opacity < 1)
            {
                tasks.Add(ThickBottomBorder.FadeTo(1, _animationLength, Easing.CubicIn));
            }

            // move the label above the editor
            var labelTranslateX = HiddenLabelControl.X - TextControl.X;
            var labelTranslateY = HiddenLabelControl.Y - TextControl.Y;
            if (Device.RuntimePlatform == Device.macOS)
            {
                labelTranslateX = -1 * labelTranslateX;
                labelTranslateY = -1 * labelTranslateY;
            }
            if (LabelControl.TranslationX != labelTranslateX || LabelControl.TranslationY != labelTranslateY)
            {
                tasks.Add(LabelControl.TranslateTo(labelTranslateX, labelTranslateY, _animationLength, Easing.CubicIn));
            }

            // update color of the label
            if (LabelControl.TextColor != AccentColor87)
            {
                tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, AccentColor87, c => LabelControl.TextColor = c, _animationLength, Easing.CubicIn));
            }

            // shrink the font size of the label
            if (LabelControl.FontSize != HiddenLabelControl.FontSize)
            {
                tasks.Add(LabelControl.FontSizeTo(LabelControl.FontSize, HiddenLabelControl.FontSize, f => LabelControl.FontSize = f, _animationLength, Easing.CubicIn));
            }

            await Task.WhenAll(tasks);
        }
    }
}
