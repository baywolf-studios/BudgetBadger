using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Forms.Animation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class MultilineTextEntry : ContentView
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

        public event EventHandler<FocusEventArgs> EditorFocused;
        public event EventHandler<FocusEventArgs> EditorUnfocused;
        public event EventHandler<TextChangedEventArgs> TextChanged;

        public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(MultilineTextEntry), defaultBindingMode: BindingMode.TwoWay);
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(MultilineTextEntry), defaultBindingMode: BindingMode.TwoWay);
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(MultilineTextEntry), defaultValue: Keyboard.Default);
        public Keyboard Keyboard
        {
            get => (Keyboard)GetValue(KeyboardProperty);
            set => SetValue(KeyboardProperty, value);
        }

        public static BindableProperty PrimaryColorProperty = BindableProperty.Create(nameof(PrimaryColor), typeof(Color), typeof(MultilineTextEntry), defaultValue: Color.Black);
        public Color PrimaryColor
        {
            get => (Color)GetValue(PrimaryColorProperty);
            set => SetValue(PrimaryColorProperty, value);
        }

        public static BindableProperty AccentColorProperty = BindableProperty.Create(nameof(AccentColor), typeof(Color), typeof(MultilineTextEntry), defaultValue: Color.Accent);
        public Color AccentColor
        {
            get => (Color)GetValue(AccentColorProperty);
            set => SetValue(AccentColorProperty, value);
        }

        public MultilineTextEntry()
        {
            InitializeComponent();

            LabelControl.BindingContext = this;
            EditorControl.BindingContext = this;

            EditorControl.Focused += (sender, e) =>
            {
                EditorFocused?.Invoke(this, e);
                UpdateVisualState();
            };

            EditorControl.Unfocused += (sender, e) =>
            {
                EditorUnfocused?.Invoke(this, e);
                UpdateVisualState();
            };

            EditorControl.TextChanged += (sender, e) =>
            {
                TextChanged?.Invoke(sender, e);
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
                EditorControl.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(EditorControl.IsFocused))
                    {
                        UpdateVisualState();
                    }
                };

                EditorControl.Completed += (sender, e) =>
                {
                    UpdateVisualState();
                };
            }

            UpdateVisualState();
        }

        void Handle_Tapped(object sender, System.EventArgs e)
        {
            if (!EditorControl.IsFocused)
            {
                EditorControl.Focus();
            }
        }

        async void UpdateVisualState()
        {
            if (EditorControl.IsFocused)
            {
                await SetFocusedState();
            }
            else if (string.IsNullOrEmpty(EditorControl.Text))
            {
                await SetIdleEmptyState();
            }
            else if (!string.IsNullOrEmpty(EditorControl.Text))
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

            if (EditorControl.TextColor != PrimaryColor38)
            {
                tasks.Add(EditorControl.ColorTo(EditorControl.TextColor, PrimaryColor38, c => EditorControl.TextColor = c, _animationLength, Easing.CubicIn));
            }

            await Task.WhenAll(tasks);
        }

        async Task SetIdleEmptyState()
        {
            var tasks = new List<Task>();

            // color the text of the editor
            if (EditorControl.TextColor != PrimaryColor87)
            {
                tasks.Add(EditorControl.ColorTo(EditorControl.TextColor, PrimaryColor87, c => EditorControl.TextColor = c, _animationLength, Easing.CubicIn));
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

            // reset the label
            if (LabelControl.TranslationX != 0 || LabelControl.TranslationY != 0)
            {
                tasks.Add(LabelControl.TranslateTo(0, 0, _animationLength, Easing.CubicIn));
            }

            // color the lable
            if (LabelControl.TextColor != PrimaryColor54)
            {
                tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, PrimaryColor54, c => LabelControl.TextColor = c, _animationLength, Easing.CubicIn));
            }

            // increase the font size of the label
            if (LabelControl.FontSize != EditorControl.FontSize)
            {
                tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, EditorControl.FontSize, f => LabelControl.FontSize = f, _animationLength, Easing.CubicIn));
            }

            await Task.WhenAll(tasks);
        }

        async Task SetIdleFilledState()
        {
            var tasks = new List<Task>();

            if (EditorControl.TextColor != PrimaryColor87)
            {
                tasks.Add(EditorControl.ColorTo(EditorControl.TextColor, PrimaryColor87, c => EditorControl.TextColor = c, _animationLength, Easing.CubicOut));
            }

            // change the color of the bottom borders
            if (BottomBorder.BackgroundColor != PrimaryColor42)
            {
                tasks.Add(BottomBorder.ColorTo(BottomBorder.BackgroundColor, PrimaryColor42, c => BottomBorder.BackgroundColor = c, _animationLength, Easing.CubicOut));
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

            // move the label above entry
            var labelTranslateX = HiddenLabelControl.X - EditorControl.X;
            var labelTranslateY = HiddenLabelControl.Y - EditorControl.Y;
            if (Device.RuntimePlatform == Device.macOS)
            {
                labelTranslateX = -1 * labelTranslateX;
                labelTranslateY = -1 * labelTranslateY;
            }
            if (LabelControl.TranslationX != labelTranslateX || LabelControl.TranslationY != labelTranslateY)
            {
                tasks.Add(LabelControl.TranslateTo(labelTranslateX, labelTranslateY, _animationLength, Easing.CubicOut));
            }

            if (LabelControl.TextColor != PrimaryColor54)
            {
                tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, PrimaryColor54, c => LabelControl.TextColor = c, _animationLength, Easing.CubicOut));
            }

            if (LabelControl.FontSize != HiddenLabelControl.FontSize)
            {
                tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, HiddenLabelControl.FontSize, f => LabelControl.FontSize = f, _animationLength, Easing.CubicOut));
            }

            await Task.WhenAll(tasks);
        }

        async Task SetFocusedState()
        {
            var tasks = new List<Task>();

            if (EditorControl.TextColor != PrimaryColor87)
            {
                tasks.Add(EditorControl.ColorTo(EditorControl.TextColor, PrimaryColor87, c => EditorControl.TextColor = c, _animationLength, Easing.CubicOut));
            }

            // set the color to the bottom borders
            if (BottomBorder.BackgroundColor != AccentColor)
            {
                tasks.Add(BottomBorder.ColorTo(BottomBorder.BackgroundColor, AccentColor, c => BottomBorder.BackgroundColor = c, _animationLength, Easing.CubicOut));
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

            var placholderTranslateX = HiddenLabelControl.X - EditorControl.X;
            var placeholderTranslateY = HiddenLabelControl.Y - EditorControl.Y;
            if (Device.RuntimePlatform == Device.macOS)
            {
                placholderTranslateX = -1 * placholderTranslateX;
                placeholderTranslateY = -1 * placeholderTranslateY;
            }
            if (LabelControl.TranslationX != placholderTranslateX || LabelControl.TranslationY != placeholderTranslateY)
            {
                tasks.Add(LabelControl.TranslateTo(placholderTranslateX, placeholderTranslateY, _animationLength, Easing.CubicOut));
            }

            if (LabelControl.TextColor != AccentColor87)
            {
                tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, AccentColor87, c => LabelControl.TextColor = c, _animationLength, Easing.CubicOut));
            }

            if (LabelControl.FontSize != HiddenLabelControl.FontSize)
            {
                tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, HiddenLabelControl.FontSize, f => LabelControl.FontSize = f, _animationLength, Easing.CubicOut));
            }

            await Task.WhenAll(tasks);
        }
    }
}
