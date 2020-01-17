using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BudgetBadger.Forms.Animation;
using BudgetBadger.Forms.Effects;
using BudgetBadger.Forms.Style;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class TextField : Grid
    {
        public static BindableProperty LabelProperty =
            BindableProperty.Create(nameof(Label),
                typeof(string),
                typeof(TextField),
                propertyChanged: (bindable, oldVal, newVal) =>
                {
                    if (bindable is TextField TextField && oldVal != newVal)
                    {
                        if (string.IsNullOrEmpty((string)newVal))
                        {
                            TextField.LabelControl.IsVisible = false;
                        }
                        else
                        {
                            TextField.LabelControl.IsVisible = true;
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
                typeof(TextField),
                defaultBindingMode: BindingMode.TwoWay,
                propertyChanged: (bindable, oldVal, newVal) =>
                {
                    if (bindable is TextField TextField && oldVal != newVal)
                    {
                        TextField.TextControl.Text = (string)newVal;
                        TextField.ReadOnlyTextControl.Text = (string)newVal;
                    }
                });
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static BindableProperty HintProperty = BindableProperty.Create(nameof(Hint), typeof(string), typeof(TextField), propertyChanged: UpdateErrorAndHint);
        public string Hint
        {
            get => (string)GetValue(HintProperty);
            set => SetValue(HintProperty, value);
        }

        public static BindableProperty ErrorProperty = BindableProperty.Create(nameof(Error), typeof(string), typeof(TextField), propertyChanged: UpdateErrorAndHint);
        public string Error
        {
            get => (string)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }

        public static BindableProperty IsPasswordProperty = BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(TextField));
        public bool IsPassword
        {
            get => (bool)GetValue(IsPasswordProperty);
            set => SetValue(IsPasswordProperty, value);
        }

        public static BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(TextField));
        public Keyboard Keyboard
        {
            get => (Keyboard)GetValue(KeyboardProperty);
            set => SetValue(KeyboardProperty, value);
        }

        public static BindableProperty IsReadOnlyProperty = BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(TextField));
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public event EventHandler Completed;

        private readonly bool _compact;

        public TextField() : this(false) { }

        public TextField(bool compact)
        {
            InitializeComponent();

            if (compact)
            {
                TextControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlEntryCompactStyle"];
                ReadOnlyTextControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlLabelCompactStyle"];
                LabelControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlDescriptionLabelCompactStyle"];
                HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlHintLabelCompactStyle"];
            }
            else
            {
                TextControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlEntryStyle"];
                ReadOnlyTextControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlLabelStyle"];
                LabelControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlDescriptionLabelStyle"];
                HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlHintLabelStyle"];
            }

            _compact = compact;

            ButtonBackground.BindingContext = this;
            LabelControl.BindingContext = this;
            TextControl.BindingContext = this;
            ReadOnlyTextControl.BindingContext = this;

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

        void Handle_Clicked(object sender, EventArgs e)
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
            if (bindable is TextField TextField && oldValue != newValue)
            {
                if (!String.IsNullOrEmpty(TextField.Error))
                {
                    TextField.HintErrorControl.IsVisible = true;
                    TextField.HintErrorControl.Text = TextField.Error;
                    if (TextField._compact)
                    {
                        TextField.HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlErrorLabelCompactStyle"];
                    }
                    else
                    {
                        TextField.HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlErrorLabelCompactStyle"];
                    }
                }
                else if (!String.IsNullOrEmpty(TextField.Hint))
                {
                    TextField.HintErrorControl.IsVisible = true;
                    TextField.HintErrorControl.Text = TextField.Hint;
                    if (TextField._compact)
                    {
                        TextField.HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlHintLabelCompactStyle"];
                    }
                    else
                    {
                        TextField.HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlHintLabelCompactStyle"];
                    }
                }
                else
                {
                    TextField.HintErrorControl.IsVisible = false;
                }
            }
        }
    }
}
