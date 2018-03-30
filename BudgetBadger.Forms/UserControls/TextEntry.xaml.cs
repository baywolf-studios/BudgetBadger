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

        public event EventHandler<FocusEventArgs> EntryFocused;
        public event EventHandler<FocusEventArgs> EntryUnfocused;
        public event EventHandler<TextChangedEventArgs> TextChanged;

        public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(TextEntry), defaultBindingMode: BindingMode.TwoWay);

        public static BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(TextEntry), defaultBindingMode: BindingMode.TwoWay, propertyChanged: (bindable, oldVal, newval) =>
        {
            var entry = (TextEntry)bindable;
            entry.LabelControl.Text = (string)newval;
        });

        public static BindableProperty IsPasswordProperty = BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(TextEntry), defaultValue: false, propertyChanged: (bindable, oldVal, newVal) =>
        {
            var matEntry = (TextEntry)bindable;
            matEntry.EntryControl.IsPassword = (bool)newVal;
        });
        public static BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(TextEntry), defaultValue: Keyboard.Default, propertyChanged: (bindable, oldVal, newVal) =>
        {
            var matEntry = (TextEntry)bindable;
            matEntry.EntryControl.Keyboard = (Keyboard)newVal;
        });

        public static BindableProperty PrimaryColorProperty = BindableProperty.Create(nameof(PrimaryColor), typeof(Color), typeof(TextEntry), defaultValue: Color.Black);
        public Color PrimaryColor
        {
            get
            {
                return (Color)GetValue(PrimaryColorProperty);
            }
            set
            {
                SetValue(PrimaryColorProperty, value);
            }
        }

        public static BindableProperty AccentColorProperty = BindableProperty.Create(nameof(AccentColor), typeof(Color), typeof(TextEntry), defaultValue: Color.Accent);
        public Color AccentColor
        {
            get
            {
                return (Color)GetValue(AccentColorProperty);
            }
            set
            {
                SetValue(AccentColorProperty, value);
            }
        }
        public Keyboard Keyboard
        {
            get
            {
                return (Keyboard)GetValue(KeyboardProperty);
            }
            set
            {
                SetValue(KeyboardProperty, value);
            }
        }

        public bool IsPassword
        {
            get
            {
                return (bool)GetValue(IsPasswordProperty);
            }
            set
            {
                SetValue(IsPasswordProperty, value);
            }
        }

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        public string Label
        {
            get
            {
                return (string)GetValue(LabelProperty);
            }
            set
            {
                SetValue(LabelProperty, value);
            }
        }

        public TextEntry()
        {
            InitializeComponent();

            EntryControl.BindingContext = this;

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

            if (BottomBorder.BackgroundColor != PrimaryColor42)
            {
                tasks.Add(BottomBorder.ColorTo(BottomBorder.BackgroundColor, PrimaryColor42, c => BottomBorder.BackgroundColor = c, 230, Easing.CubicIn));
            }

            if (LabelControl.TextColor != PrimaryColor38)
            {
                tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, PrimaryColor42, c => LabelControl.TextColor = c, 230, Easing.CubicIn));
            }

            if (EntryControl.TextColor != PrimaryColor38)
            {
                tasks.Add(EntryControl.ColorTo(EntryControl.TextColor, PrimaryColor38, c => EntryControl.TextColor = c, 230, Easing.CubicIn));
            }

            await Task.WhenAll(tasks);
        }

        async Task SetIdleEmptyState()
        {
            var tasks = new List<Task>();

            if (EntryControl.TextColor != PrimaryColor87)
            {
                tasks.Add(EntryControl.ColorTo(EntryControl.TextColor, PrimaryColor87, c => EntryControl.TextColor = c, 230, Easing.CubicIn));
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

            if (LabelControl.FontSize != EntryControl.FontSize)
            {
                tasks.Add(LabelControl.FontSizeTo(LabelControl.FontSize, EntryControl.FontSize, f => LabelControl.FontSize = f, 230, Easing.CubicIn));
            }

            await Task.WhenAll(tasks);
            BottomBorder.HeightRequest = 1;
        }

        async Task SetIdleFilledState()
        {
            var tasks = new List<Task>();

            if (EntryControl.TextColor != PrimaryColor87)
            {
                tasks.Add(EntryControl.ColorTo(EntryControl.TextColor, PrimaryColor87, c => EntryControl.TextColor = c, 230, Easing.CubicIn));
            }

            if (BottomBorder.BackgroundColor != PrimaryColor42)
            {
                tasks.Add(BottomBorder.ColorTo(BottomBorder.BackgroundColor, PrimaryColor42, c => BottomBorder.BackgroundColor = c, 230, Easing.CubicIn));
            }

            var placholderTranslateX = HiddenLabelControl.X - EntryControl.X;
            var placeholderTranslateY = HiddenLabelControl.Y - EntryControl.Y;
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

            if (EntryControl.TextColor != PrimaryColor87)
            {
                tasks.Add(EntryControl.ColorTo(EntryControl.TextColor, PrimaryColor87, c => EntryControl.TextColor = c, 230, Easing.CubicIn));
            }

            if (BottomBorder.BackgroundColor != AccentColor)
            {
                tasks.Add(BottomBorder.ColorTo(BottomBorder.BackgroundColor, AccentColor, c => BottomBorder.BackgroundColor = c, 230, Easing.CubicIn));
            }

            var placholderTranslateX = HiddenLabelControl.X - EntryControl.X;
            var placeholderTranslateY = HiddenLabelControl.Y - EntryControl.Y;
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
