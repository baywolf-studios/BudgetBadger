using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BudgetBadger.Forms.Animation;
using BudgetBadger.Forms.Effects;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class TextEntry : StackLayout
    {
        public static BindableProperty LabelProperty =
            BindableProperty.Create(nameof(Label),
                typeof(string),
                typeof(TextEntry),
                propertyChanged: (bindable, oldVal, newVal) =>
                {
                    if (bindable is TextEntry textEntry && oldVal != newVal)
                    {
                        if (string.IsNullOrEmpty((string)newVal))
                        {
                            textEntry.LabelControl.IsVisible = false;
                        }
                        else
                        {
                            textEntry.LabelControl.IsVisible = true;
                        }
                    }
                });
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text),
                typeof(string),
                typeof(TextEntry),
                defaultBindingMode: BindingMode.TwoWay,
                propertyChanged: (bindable, oldVal, newVal) =>
                {
                    if (bindable is TextEntry textEntry && oldVal != newVal)
                    {
                        textEntry.TextControl.Text = (string)newVal;
                    }
                });
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static BindableProperty HintProperty = BindableProperty.Create(nameof(Hint), typeof(string), typeof(TextEntry), propertyChanged: UpdateErrorAndHint);
        public string Hint
        {
            get => (string)GetValue(HintProperty);
            set => SetValue(HintProperty, value);
        }

        public static BindableProperty ErrorProperty = BindableProperty.Create(nameof(Error), typeof(string), typeof(TextEntry), propertyChanged: UpdateErrorAndHint);
        public string Error
        {
            get => (string)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }

        public static BindableProperty IsPasswordProperty = BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(TextEntry));
        public bool IsPassword
        {
            get => (bool)GetValue(IsPasswordProperty);
            set => SetValue(IsPasswordProperty, value);
        }

        public static BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(TextEntry));
        public Keyboard Keyboard
        {
            get => (Keyboard)GetValue(KeyboardProperty);
            set => SetValue(KeyboardProperty, value);
        }

        public static BindableProperty IsReadOnlyProperty = BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(TextEntry));
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public static BindableProperty IsBorderlessProperty =
            BindableProperty.Create(nameof(IsBorderless),
                typeof(bool),
                typeof(TextEntry),
                propertyChanged: (bindable, oldVal, newVal) =>
                {
                    if (bindable is TextEntry textEntry && oldVal != newVal)
                    {
                        if ((bool)newVal)
                        {
                            textEntry.TextControl.Effects.Add(new BorderlessEntryEffect());
                        }
                        else
                        {
                            var toRemove = textEntry.TextControl.Effects.FirstOrDefault(e => e is BorderlessEntryEffect);
                            if (toRemove != null)
                            {
                                textEntry.TextControl.Effects.Remove(toRemove);
                            }
                        }
                    }
                });
        public bool IsBorderless
        {
            get => (bool)GetValue(IsBorderlessProperty);
            set => SetValue(IsBorderlessProperty, value);
        }

        public event EventHandler Completed;

        public TextEntry()
        {
            InitializeComponent();
            
            LabelControl.BindingContext = this;
            TextControl.BindingContext = this;

            Focused += Focused2;

            TextControl.Focused += Control_Focused;
            TextControl.Unfocused += TextControl_Completed;
            TextControl.Completed += TextControl_Completed;

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    TextControl.IsEnabled = IsEnabled;
                }
            };
        }

        void Focused2(object sender, FocusEventArgs e)
        {
            if (!IsReadOnly && IsEnabled && !TextControl.IsFocused)
            {
                TextControl.Focus();
            }
        }

        void Control_Focused(object sender, FocusEventArgs e)
        {
            if (IsReadOnly || !IsEnabled)
            {
                TextControl.Unfocus();
            }
        }

        void TextControl_Completed(object sender, EventArgs e)
        {
            if (Text != TextControl.Text)
            {
                Text = TextControl.Text;
                Completed?.Invoke(this, new EventArgs());
            }
        }

        static void UpdateErrorAndHint(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is TextEntry textEntry && oldValue != newValue)
            {
                if (!String.IsNullOrEmpty(textEntry.Error))
                {
                    textEntry.HintErrorControl.IsVisible = true;
                    textEntry.HintErrorControl.Text = textEntry.Error;
                    textEntry.HintErrorControl.TextColor = (Color)Application.Current.Resources["ErrorColor"];
                }
                else if (!String.IsNullOrEmpty(textEntry.Hint))
                {
                    textEntry.HintErrorControl.IsVisible = true;
                    textEntry.HintErrorControl.Text = textEntry.Hint;
                    textEntry.HintErrorControl.TextColor = (Color)Application.Current.Resources["IdleColor"];
                }
                else
                {
                    textEntry.HintErrorControl.IsVisible = false;
                }
            }
        }
    }
}
