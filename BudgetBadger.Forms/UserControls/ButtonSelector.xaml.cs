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

        public event EventHandler<TextChangedEventArgs> TextChanged;

        public static BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ButtonSelector), defaultBindingMode: BindingMode.TwoWay);
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(ButtonSelector), defaultBindingMode: BindingMode.TwoWay);
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static BindableProperty LabelTextProperty = BindableProperty.Create(nameof(LabelText), typeof(string), typeof(ButtonSelector), defaultBindingMode: BindingMode.TwoWay);
        public string LabelText
        {
            get => (string)GetValue(LabelTextProperty);
            set => SetValue(LabelTextProperty, value);
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

            if (!IsEnabled)
            {
                await UpdateDisabled();
            }
        }

        async Task UpdateDisabled()
        {
            var tasks = new List<Task>();

            if (BottomBorder.BackgroundColor != PrimaryColor42)
            {
                tasks.Add(BottomBorder.ColorTo(BottomBorder.BackgroundColor, PrimaryColor42, c => BottomBorder.BackgroundColor = c, 230, Easing.CubicIn));
            }

            if (LabelControl.TextColor != PrimaryColor38)
            {
                tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, PrimaryColor42, c => LabelControl.TextColor = c, 230, Easing.CubicIn));
            }

            if (TextControl.TextColor != PrimaryColor38)
            {
                tasks.Add(TextControl.ColorTo(TextControl.TextColor, PrimaryColor38, c => TextControl.TextColor = c, 230, Easing.CubicIn));
            }

            await Task.WhenAll(tasks);
        }

        async Task SetIdleEmptyState()
        {
            var tasks = new List<Task>();

            if (TextControl.TextColor != PrimaryColor87)
            {
                tasks.Add(TextControl.ColorTo(TextControl.TextColor, PrimaryColor87, c => TextControl.TextColor = c, 230, Easing.CubicIn));
            }

            if (BottomBorder.BackgroundColor != PrimaryColor42)
            {
                tasks.Add(BottomBorder.ColorTo(BottomBorder.BackgroundColor, PrimaryColor42, c => BottomBorder.BackgroundColor = c, 230, Easing.CubicIn));
            }

            if (LabelControl.TranslationX != 0 || LabelControl.TranslationY != 0)
            {
                tasks.Add(LabelControl.TranslateTo(0, 0, 230, Easing.CubicIn));
            }

            if (LabelControl.TextColor != PrimaryColor54)
            {
                tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, PrimaryColor54, c => LabelControl.TextColor = c, 230, Easing.CubicIn));
            }

            if (LabelControl.FontSize != TextControl.FontSize)
            {
                tasks.Add(LabelControl.FontSizeTo(LabelControl.FontSize, TextControl.FontSize, f => LabelControl.FontSize = f, 230, Easing.CubicIn));
            }

            await Task.WhenAll(tasks);
            BottomBorder.HeightRequest = 1;
        }

        async Task SetIdleFilledState()
        {
            var tasks = new List<Task>();

            if (TextControl.TextColor != PrimaryColor87)
            {
                tasks.Add(TextControl.ColorTo(TextControl.TextColor, PrimaryColor87, c => TextControl.TextColor = c, 230, Easing.CubicIn));
            }

            if (BottomBorder.BackgroundColor != PrimaryColor42)
            {
                tasks.Add(BottomBorder.ColorTo(BottomBorder.BackgroundColor, PrimaryColor42, c => BottomBorder.BackgroundColor = c, 230, Easing.CubicIn));
            }

            var placholderTranslateX = HiddenLabelControl.X - TextControl.X;
            var placeholderTranslateY = HiddenLabelControl.Y - TextControl.Y;
            if (Device.RuntimePlatform == Device.macOS)
            {
                placholderTranslateX = -1 * placholderTranslateX;
                placeholderTranslateY = -1 * placeholderTranslateY;
            }
            if (LabelControl.TranslationX != placholderTranslateX || LabelControl.TranslationY != placeholderTranslateY)
            {
                tasks.Add(LabelControl.TranslateTo(placholderTranslateX, placeholderTranslateY, 230, Easing.CubicIn));
            }

            if (LabelControl.TextColor != PrimaryColor54)
            {
                tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, PrimaryColor54, c => LabelControl.TextColor = c, 230, Easing.CubicIn));
            }

            if (LabelControl.FontSize != HiddenLabelControl.FontSize)
            {
                tasks.Add(LabelControl.FontSizeTo(LabelControl.FontSize, HiddenLabelControl.FontSize, f => LabelControl.FontSize = f, 230, Easing.CubicIn));
            }

            await Task.WhenAll(tasks);
            BottomBorder.HeightRequest = 1;
        }

        async Task SetFocusedState()
        {
            var tasks = new List<Task>();

            if (TextControl.TextColor != PrimaryColor87)
            {
                tasks.Add(TextControl.ColorTo(TextControl.TextColor, PrimaryColor87, c => TextControl.TextColor = c, 230, Easing.CubicIn));
            }

            if (BottomBorder.BackgroundColor != AccentColor)
            {
                tasks.Add(BottomBorder.ColorTo(BottomBorder.BackgroundColor, AccentColor, c => BottomBorder.BackgroundColor = c, 230, Easing.CubicIn));
            }

            var placholderTranslateX = HiddenLabelControl.X - TextControl.X;
            var placeholderTranslateY = HiddenLabelControl.Y - TextControl.Y;
            if (Device.RuntimePlatform == Device.macOS)
            {
                placholderTranslateX = -1 * placholderTranslateX;
                placeholderTranslateY = -1 * placeholderTranslateY;
            }
            if (LabelControl.TranslationX != placholderTranslateX || LabelControl.TranslationY != placeholderTranslateY)
            {
                tasks.Add(LabelControl.TranslateTo(placholderTranslateX, placeholderTranslateY, 230, Easing.CubicIn));
            }

            if (LabelControl.TextColor != AccentColor87)
            {
                tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, AccentColor87, c => LabelControl.TextColor = c, 230, Easing.CubicIn));
            }

            if (LabelControl.FontSize != HiddenLabelControl.FontSize)
            {
                tasks.Add(LabelControl.FontSizeTo(LabelControl.FontSize, HiddenLabelControl.FontSize, f => LabelControl.FontSize = f, 230, Easing.CubicIn));
            }

            await Task.WhenAll(tasks);
            BottomBorder.HeightRequest = 2;
        }
    }
}
