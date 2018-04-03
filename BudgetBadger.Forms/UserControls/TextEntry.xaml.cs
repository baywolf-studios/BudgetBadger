using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BudgetBadger.Forms.Animation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class TextEntry : ContentView
    {
        uint _animationLength = 150;

        Color _primaryColor38
        {
            get => Color.FromRgba(PrimaryColor.R, PrimaryColor.G, PrimaryColor.B, 0.38);
        }

        Color _primaryColor42 
        {
            get => Color.FromRgba(PrimaryColor.R, PrimaryColor.G, PrimaryColor.B, 0.42);
        }

        Color _primaryColor54
        {
            get => Color.FromRgba(PrimaryColor.R, PrimaryColor.G, PrimaryColor.B, 0.54);
        }

        Color _primaryColor87
        {
            get => Color.FromRgba(PrimaryColor.R, PrimaryColor.G, PrimaryColor.B, 0.87);
        }

        Color _accentColor87
        {
            get => Color.FromRgba(AccentColor.R, AccentColor.G, AccentColor.B, 0.87);
        }

        public static void Init() { }

        public event EventHandler<FocusEventArgs> EntryFocused;
        public event EventHandler<FocusEventArgs> EntryUnfocused;
        public event EventHandler<TextChangedEventArgs> TextChanged;

        public static BindableProperty PrefixProperty = BindableProperty.Create(nameof(Prefix), typeof(string), typeof(TextEntry), defaultBindingMode: BindingMode.TwoWay);
        public string Prefix
        {
            get => (string)GetValue(PrefixProperty);
            set => SetValue(PrefixProperty, value);
        }

        public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(TextEntry), defaultBindingMode: BindingMode.TwoWay);
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(TextEntry), defaultBindingMode: BindingMode.TwoWay);
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
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

        public static BindableProperty PrimaryColorProperty = BindableProperty.Create(nameof(PrimaryColor), typeof(Color), typeof(TextEntry), defaultValue: Color.Black);
        public Color PrimaryColor
        {
            get => (Color)GetValue(PrimaryColorProperty);
            set => SetValue(PrimaryColorProperty, value);
        }

        public static BindableProperty AccentColorProperty = BindableProperty.Create(nameof(AccentColor), typeof(Color), typeof(TextEntry), defaultValue: Color.Accent);
        public Color AccentColor
        {
            get => (Color)GetValue(AccentColorProperty);
            set => SetValue(AccentColorProperty, value);
        }

        public TextEntry()
        {
            InitializeComponent();

            LabelControl.BindingContext = this;
            EntryControl.BindingContext = this;
            PrefixControl.BindingContext = this;

            EntryControl.Focused += (sender, e) =>
            {
                EntryFocused?.Invoke(this, e);
                UpdateVisualState();
            };

            EntryControl.Unfocused += (sender, e) =>
            {
                EntryUnfocused?.Invoke(this, e);
                UpdateVisualState();
            };

            EntryControl.TextChanged += (sender, e) => 
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
                EntryControl.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(EntryControl.IsFocused))
                    {
                        UpdateVisualState();
                    }
                };

                EntryControl.Completed += (sender, e) =>
                {
                    UpdateVisualState();
                };
            }

            UpdateVisualState();
        }

        void Handle_Tapped(object sender, System.EventArgs e)
        {
            if (!EntryControl.IsFocused)
            {
                EntryControl.Focus();
            }
        }

        async void UpdateVisualState()
        {
            if (EntryControl.IsFocused)
            {
                await SetFocusedState();
            }
            else if (string.IsNullOrEmpty(EntryControl.Text))
            {
                await SetIdleEmptyState();
            }
            else if (!string.IsNullOrEmpty(EntryControl.Text))
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

            if (BottomBorder.BackgroundColor != _primaryColor42)
            {
                tasks.Add(BottomBorder.ColorTo(BottomBorder.BackgroundColor, _primaryColor42, c => BottomBorder.BackgroundColor = c, _animationLength, Easing.CubicIn));
            }

            if (ThickBottomBorder.BackgroundColor != _primaryColor42)
            {
                tasks.Add(ThickBottomBorder.ColorTo(ThickBottomBorder.BackgroundColor, _primaryColor42, c => ThickBottomBorder.BackgroundColor = c, _animationLength, Easing.CubicIn));
            }

            if (PrefixControl.TextColor != _primaryColor38)
            {
                tasks.Add(PrefixControl.ColorTo(PrefixControl.TextColor, _primaryColor38, c => PrefixControl.TextColor = c, _animationLength, Easing.CubicIn));
            }

            if (LabelControl.TextColor != _primaryColor38)
            {
                tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, _primaryColor38, c => LabelControl.TextColor = c, _animationLength, Easing.CubicIn));
            }

            if (EntryControl.TextColor != _primaryColor38)
            {
                tasks.Add(EntryControl.ColorTo(EntryControl.TextColor, _primaryColor38, c => EntryControl.TextColor = c, _animationLength, Easing.CubicIn));
            }

            await Task.WhenAll(tasks);
        }

        async Task SetIdleEmptyState()
        {
            var tasks = new List<Task>();

            // prefix should be hidden
            if (PrefixControl.Opacity > 0)
            {
                tasks.Add(PrefixControl.FadeTo(0, _animationLength, Easing.CubicIn));
            }

            // set the idle color of the entry
            if (EntryControl.TextColor != _primaryColor87)
            {
                tasks.Add(EntryControl.ColorTo(EntryControl.TextColor, _primaryColor87, c => EntryControl.TextColor = c, _animationLength, Easing.CubicIn));
            }

            // set the idle color of the bottom borders
            if (BottomBorder.BackgroundColor != _primaryColor42)
            {
                tasks.Add(BottomBorder.ColorTo(BottomBorder.BackgroundColor, _primaryColor42, c => BottomBorder.BackgroundColor = c, _animationLength, Easing.CubicIn));
            }

            if (ThickBottomBorder.BackgroundColor != _primaryColor42)
            {
                tasks.Add(ThickBottomBorder.ColorTo(ThickBottomBorder.BackgroundColor, _primaryColor42, c => ThickBottomBorder.BackgroundColor = c, _animationLength, Easing.CubicIn));
            }

            // show the normal bottom border
            if (BottomBorder.Opacity < 1)
            {
                tasks.Add(BottomBorder.FadeTo(1, _animationLength, Easing.CubicIn));
            }

            // hide the thick bottom border
            if (ThickBottomBorder.Opacity > 0)
            {
                tasks.Add(ThickBottomBorder.FadeTo(0, _animationLength, Easing.CubicIn));
            }

            // reset label position
            if (LabelControl.TranslationX != 0 || LabelControl.TranslationY != 0)
            {
                tasks.Add(LabelControl.TranslateTo(0, 0, _animationLength, Easing.CubicIn));
            }

            // set the idle label color
            if (LabelControl.TextColor != _primaryColor54)
            {
                tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, _primaryColor54, c => LabelControl.TextColor = c, _animationLength, Easing.CubicIn));
            }

            // set the font size of the label
            if (LabelControl.FontSize != EntryControl.FontSize)
            {
                tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, EntryControl.FontSize, f => LabelControl.FontSize = f, _animationLength, Easing.CubicIn));
            }

            await Task.WhenAll(tasks);
        }

        async Task SetIdleFilledState()
        {
            var tasks = new List<Task>();

            // show the prefix if there is one
            if (!string.IsNullOrEmpty(Prefix) && PrefixControl.Opacity < 1)
            {
                tasks.Add(PrefixControl.FadeTo(1, _animationLength, Easing.CubicOut));
            }

            // set the prefix color if exists
            if (!string.IsNullOrEmpty(Prefix) && PrefixControl.TextColor != _primaryColor54)
            {
                tasks.Add(PrefixControl.ColorTo(PrefixControl.TextColor, _primaryColor54, c => PrefixControl.TextColor = c, _animationLength, Easing.CubicOut));
            }

            // set teh entry color
            if (EntryControl.TextColor != _primaryColor87)
            {
                tasks.Add(EntryControl.ColorTo(EntryControl.TextColor, _primaryColor87, c => EntryControl.TextColor = c, _animationLength, Easing.CubicOut));
            }

            // set the bottom border color
            if (BottomBorder.BackgroundColor != _primaryColor42)
            {
                tasks.Add(BottomBorder.ColorTo(BottomBorder.BackgroundColor, _primaryColor42, c => BottomBorder.BackgroundColor = c, _animationLength, Easing.CubicOut));
            }

            // set the thick bottom border color
            if (ThickBottomBorder.BackgroundColor != _primaryColor42)
            {
                tasks.Add(ThickBottomBorder.ColorTo(ThickBottomBorder.BackgroundColor, _primaryColor42, c => ThickBottomBorder.BackgroundColor = c, _animationLength, Easing.CubicOut));
            }

            // show the normal bottom border
            if (BottomBorder.Opacity < 1)
            {
                tasks.Add(BottomBorder.FadeTo(1, _animationLength, Easing.CubicOut));
            }

            // hide the thick bottom border
            if (ThickBottomBorder.Opacity > 0)
            {
                tasks.Add(ThickBottomBorder.FadeTo(0, _animationLength, Easing.CubicOut));
            }

            //move the label above the entry and prefix
            var labelTranslateY = HiddenLabelControl.Y - EntryControl.Y;

            if (Device.RuntimePlatform == Device.macOS)
            {
                labelTranslateY = -1 * labelTranslateY;
            }
            if (LabelControl.TranslationY != labelTranslateY)
            {
                tasks.Add(LabelControl.TranslateTo(0, labelTranslateY, _animationLength, Easing.CubicOut));
            }

            //move the entry to the the right if the prefix is there
            var entryTranslateX = (double)0;
            if (!string.IsNullOrEmpty(Prefix))
            {
                entryTranslateX = PrefixControl.X + PrefixControl.Width + 8;
            }
            if (EntryControl.TranslationX != entryTranslateX)
            {
                tasks.Add(EntryControl.TranslateTo(entryTranslateX, EntryControl.TranslationY, _animationLength, Easing.CubicOut));
            }

            // set the color of the label
            if (LabelControl.TextColor != _primaryColor54)
            {
                tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, _primaryColor54, c => LabelControl.TextColor = c, _animationLength, Easing.CubicOut));
            }

            // make the label font size smaller
            if (LabelControl.FontSize != HiddenLabelControl.FontSize)
            {
                tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, HiddenLabelControl.FontSize, f => LabelControl.FontSize = f, _animationLength, Easing.CubicOut));
            }

            await Task.WhenAll(tasks);
        }

        async Task SetFocusedState()
        {
            var tasks = new List<Task>();

            // show the prefix if there is one
            if (!string.IsNullOrEmpty(Prefix) && PrefixControl.Opacity < 1)
            {
                tasks.Add(PrefixControl.FadeTo(1, _animationLength, Easing.CubicOut));
            }

            // set the prefix color if it exists
            if (!string.IsNullOrEmpty(Prefix) && PrefixControl.TextColor != _primaryColor54)
            {
                tasks.Add(PrefixControl.ColorTo(PrefixControl.TextColor, _primaryColor54, c => PrefixControl.TextColor = c, _animationLength, Easing.CubicOut));
            }

            // set the entry color
            if (EntryControl.TextColor != _primaryColor87)
            {
                tasks.Add(EntryControl.ColorTo(EntryControl.TextColor, _primaryColor87, c => EntryControl.TextColor = c, _animationLength, Easing.CubicOut));
            }

            // set the normal bottom border color
            if (BottomBorder.BackgroundColor != AccentColor)
            {
                tasks.Add(BottomBorder.ColorTo(BottomBorder.BackgroundColor, AccentColor, c => BottomBorder.BackgroundColor = c, _animationLength, Easing.CubicOut));
            }

            // set the thick bottom border color
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
                tasks.Add(ThickBottomBorder.FadeTo(1, _animationLength, Easing.CubicOut));
            }

            //move the label above the entry and prefix
            var labelTranslateY = HiddenLabelControl.Y - EntryControl.Y;
            if (Device.RuntimePlatform == Device.macOS)
            {
                labelTranslateY = -1 * labelTranslateY;
            }
            if (LabelControl.TranslationY != labelTranslateY)
            {
                tasks.Add(LabelControl.TranslateTo(0, labelTranslateY, _animationLength, Easing.CubicOut));
            }

            //move the entry to the the right if the prefix is there
            var entryTranslateX = (double)0;
            if (!string.IsNullOrEmpty(Prefix))
            {
                entryTranslateX = PrefixControl.X + PrefixControl.Width + 8;
            }
            if (EntryControl.TranslationX != entryTranslateX)
            {
                tasks.Add(EntryControl.TranslateTo(entryTranslateX, EntryControl.TranslationY, _animationLength, Easing.CubicOut));
            }

            // set the label color
            if (LabelControl.TextColor != _accentColor87)
            {
                tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, _accentColor87, c => LabelControl.TextColor = c, _animationLength, Easing.CubicOut));
            }

            // shrink the label font size
            if (LabelControl.FontSize != HiddenLabelControl.FontSize)
            {
                tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, HiddenLabelControl.FontSize, f => LabelControl.FontSize = f, _animationLength, Easing.CubicOut));
            }

            await Task.WhenAll(tasks);
        }
    }
}
