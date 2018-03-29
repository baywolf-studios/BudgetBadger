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
                if (string.IsNullOrEmpty(MainEntry.Text))
                await Task.WhenAll(
                    BottomBorder.ColorTo(Color.FromRgba(0, 0, 0, .56), ActiveColor, c => BottomBorder.BackgroundColor = c, 230, Easing.CubicOut),
                    PlaceholderLabel.TranslateTo(HiddenLabel.X - MainEntry.X, HiddenLabel.Y - MainEntry.Y, 230, Easing.CubicOut),
                    PlaceholderLabel.ColorTo(Color.FromRgba(0,0,0,.56), ActiveColor, c => PlaceholderLabel.TextColor = c, 230, Easing.CubicOut),
                    PlaceholderLabel.FontSizeTo(MainEntry.FontSize, HiddenLabel.FontSize, f => PlaceholderLabel.FontSize = f, 230, Easing.CubicOut)
                );
            };
            MainEntry.Unfocused += async (s, a) =>
            {
                if (string.IsNullOrEmpty(MainEntry.Text))
                {
                    await Task.WhenAll(
                        BottomBorder.ColorTo(ActiveColor, Color.FromRgba(0, 0, 0, .56), c => BottomBorder.BackgroundColor = c, 230, Easing.CubicIn),
                        PlaceholderLabel.TranslateTo(0, 0, 230, Easing.CubicIn),
                        PlaceholderLabel.ColorTo(ActiveColor, Color.FromRgba(0, 0, 0, .56), c => PlaceholderLabel.TextColor = c, 230, Easing.CubicIn),
                        PlaceholderLabel.FontSizeTo(HiddenLabel.FontSize, MainEntry.FontSize, f => PlaceholderLabel.FontSize = f, 230, Easing.CubicIn)
                    );
                }
            };
        }
    }
}
