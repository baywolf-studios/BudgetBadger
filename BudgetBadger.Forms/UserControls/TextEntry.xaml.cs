using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Forms.Animations;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class TextEntry : ContentView
    {
        Color TextColor = Color.Black;
        Color InactiveColor = Color.FromRgba(0, 0, 0, .56);

        public static void Init() { }
        public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(TextEntry), defaultBindingMode: BindingMode.TwoWay);
        public static BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(TextEntry), defaultBindingMode: BindingMode.TwoWay, propertyChanged: (bindable, oldVal, newval) =>
        {
            var matEntry = (TextEntry)bindable;
            //matEntry.MainEntry.Placeholder = (string)newval;
            matEntry.PlaceholderLabel.Text = (string)newval;
        });

        public static BindableProperty IsPasswordProperty = BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(TextEntry), defaultValue: false, propertyChanged: (bindable, oldVal, newVal) =>
        {
            var matEntry = (TextEntry)bindable;
            matEntry.MainEntry.IsPassword = (bool)newVal;
        });
        public static BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(TextEntry), defaultValue: Keyboard.Default, propertyChanged: (bindable, oldVal, newVal) =>
        {
            var matEntry = (TextEntry)bindable;
            matEntry.MainEntry.Keyboard = (Keyboard)newVal;
        });
        public static BindableProperty ActiveColorProperty = BindableProperty.Create(nameof(ActiveColor), typeof(Color), typeof(TextEntry), defaultValue: Color.Accent);
        public Color ActiveColor
        {
            get
            {
                return (Color)GetValue(ActiveColorProperty);
            }
            set
            {
                SetValue(ActiveColorProperty, value);
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
        public string Placeholder
        {
            get
            {
                return (string)GetValue(PlaceholderProperty);
            }
            set
            {
                SetValue(PlaceholderProperty, value);
            }
        }

        public TextEntry()
        {
            InitializeComponent();

            MainEntry.BindingContext = this;
            MainEntry.Focused += async (s, a) =>
            {
                await SetFocusedState();
            };
            MainEntry.Unfocused += async (s, a) =>
            {
                if (string.IsNullOrEmpty(MainEntry.Text))
                {
                    await SetIdleEmptyState();
                }
                else
                {
                    await SetIdleFilledState();
                }
            };
            MainEntry.TextChanged += async (sender, e) => 
            {
                if (MainEntry.IsFocused)
                {
                    await SetFocusedState();
                }
                else if (string.IsNullOrEmpty(MainEntry.Text))
                {
                    await SetIdleEmptyState();
                }
                else if (!string.IsNullOrEmpty(MainEntry.Text))
                {
                    await SetIdleFilledState();
                }
            };
        }

        async Task SetIdleEmptyState()
        {
            var tasks = new List<Task>();

            if (BottomBorder.BackgroundColor != Color.FromRgba(0, 0, 0, .56))
            {
                tasks.Add(BottomBorder.ColorTo(BottomBorder.BackgroundColor, Color.FromRgba(0, 0, 0, .56), c => BottomBorder.BackgroundColor = c, 230, Easing.CubicIn));
            }

            if (PlaceholderLabel.TranslationX != 0 || PlaceholderLabel.TranslationY != 0)
            {
                tasks.Add(PlaceholderLabel.TranslateTo(0, 0, 230, Easing.CubicIn));
            }

            if (PlaceholderLabel.TextColor != Color.FromRgba(0,0,0,.56))
            {
                tasks.Add(PlaceholderLabel.ColorTo(PlaceholderLabel.TextColor, Color.FromRgba(0, 0, 0, .56), c => PlaceholderLabel.TextColor = c, 230, Easing.CubicIn));
            }

            if (PlaceholderLabel.FontSize != MainEntry.FontSize)
            {
                tasks.Add(PlaceholderLabel.FontSizeTo(PlaceholderLabel.FontSize, MainEntry.FontSize, f => PlaceholderLabel.FontSize = f, 230, Easing.CubicIn));
            }

            await Task.WhenAll(tasks);
        }

        async Task SetIdleFilledState()
        {
            var tasks = new List<Task>();

            if (BottomBorder.BackgroundColor != Color.FromRgba(0, 0, 0, .56))
            {
                tasks.Add(BottomBorder.ColorTo(BottomBorder.BackgroundColor, Color.FromRgba(0, 0, 0, .56), c => BottomBorder.BackgroundColor = c, 230, Easing.CubicIn));
            }

            var placholderTranslateX = HiddenLabel.X - MainEntry.X;
            var placeholderTranslateY = HiddenLabel.Y - MainEntry.Y;
            if (PlaceholderLabel.TranslationX != placholderTranslateX || PlaceholderLabel.TranslationY != placeholderTranslateY)
            {
                tasks.Add(PlaceholderLabel.TranslateTo(placholderTranslateX, placeholderTranslateY, 230, Easing.CubicIn));
            }

            if (PlaceholderLabel.TextColor != Color.FromRgba(0, 0, 0, .56))
            {
                tasks.Add(PlaceholderLabel.ColorTo(PlaceholderLabel.TextColor, Color.FromRgba(0, 0, 0, .56), c => PlaceholderLabel.TextColor = c, 230, Easing.CubicIn));
            }

            if (PlaceholderLabel.FontSize != HiddenLabel.FontSize)
            {
                tasks.Add(PlaceholderLabel.FontSizeTo(PlaceholderLabel.FontSize, HiddenLabel.FontSize, f => PlaceholderLabel.FontSize = f, 230, Easing.CubicIn));
            }

            await Task.WhenAll(tasks);
        }

        async Task SetFocusedState()
        {
            var tasks = new List<Task>();

            if (BottomBorder.BackgroundColor != ActiveColor)
            {
                tasks.Add(BottomBorder.ColorTo(BottomBorder.BackgroundColor, ActiveColor, c => BottomBorder.BackgroundColor = c, 230, Easing.CubicIn));
            }

            var placholderTranslateX = HiddenLabel.X - MainEntry.X;
            var placeholderTranslateY = HiddenLabel.Y - MainEntry.Y;
            if (PlaceholderLabel.TranslationX != placholderTranslateX || PlaceholderLabel.TranslationY != placeholderTranslateY)
            {
                tasks.Add(PlaceholderLabel.TranslateTo(placholderTranslateX, placeholderTranslateY, 230, Easing.CubicIn));
            }

            if (PlaceholderLabel.TextColor != ActiveColor)
            {
                tasks.Add(PlaceholderLabel.ColorTo(PlaceholderLabel.TextColor, ActiveColor, c => PlaceholderLabel.TextColor = c, 230, Easing.CubicIn));
            }

            if (PlaceholderLabel.FontSize != HiddenLabel.FontSize)
            {
                tasks.Add(PlaceholderLabel.FontSizeTo(PlaceholderLabel.FontSize, HiddenLabel.FontSize, f => PlaceholderLabel.FontSize = f, 230, Easing.CubicIn));
            }

            await Task.WhenAll(tasks);
        }
    }
}
