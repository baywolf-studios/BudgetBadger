﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Forms.Animation;
using BudgetBadger.Forms.Effects;
using BudgetBadger.Forms.Style;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class CurrencyCalculatorEntry : Grid
    {
        public static BindableProperty LabelProperty =
            BindableProperty.Create(nameof(Label),
                typeof(string),
                typeof(CurrencyCalculatorEntry),
                propertyChanged: (bindable, oldVal, newVal) =>
                {
                    if (bindable is CurrencyCalculatorEntry TextField && oldVal != newVal)
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

        public static BindableProperty NumberProperty =
            BindableProperty.Create(nameof(Number),
                typeof(decimal?),
                typeof(CurrencyCalculatorEntry),
                defaultBindingMode: BindingMode.TwoWay,
                propertyChanged: (bindable, oldVal, newVal) =>
                {
                    if (bindable is CurrencyCalculatorEntry TextField && oldVal != newVal)
                    {
                        TextField.TextControl.Number = (decimal?)newVal;
                    }
                });
        public decimal? Number
        {
            get => (decimal?)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }

        public static BindableProperty HintProperty = BindableProperty.Create(nameof(Hint), typeof(string), typeof(CurrencyCalculatorEntry), propertyChanged: UpdateErrorAndHint);
        public string Hint
        {
            get => (string)GetValue(HintProperty);
            set => SetValue(HintProperty, value);
        }

        public static BindableProperty ErrorProperty = BindableProperty.Create(nameof(Error), typeof(string), typeof(CurrencyCalculatorEntry), propertyChanged: UpdateErrorAndHint);
        public string Error
        {
            get => (string)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }

        public static BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(CurrencyCalculatorEntry));
        public Keyboard Keyboard
        {
            get => (Keyboard)GetValue(KeyboardProperty);
            set => SetValue(KeyboardProperty, value);
        }

        public static BindableProperty IsReadOnlyProperty = BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(CurrencyCalculatorEntry));
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public TextAlignment HorizontalTextAlignment
        {
            get => TextControl.HorizontalTextAlignment;
            set
            {
                TextControl.HorizontalTextAlignment = value;
                ReadOnlyTextControl.HorizontalTextAlignment = value;
            }
        }

        public event EventHandler Completed;

        private bool _compact;

        public CurrencyCalculatorEntry() : this(false) { }

        public CurrencyCalculatorEntry(bool compact)
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
            if (Number != TextControl.Number)
            {
                Number = TextControl.Number;
                Completed?.Invoke(this, new EventArgs());
            }
        }

        static void UpdateErrorAndHint(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CurrencyCalculatorEntry TextField && oldValue != newValue)
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
