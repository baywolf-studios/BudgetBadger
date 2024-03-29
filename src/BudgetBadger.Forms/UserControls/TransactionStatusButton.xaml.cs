﻿using System;
using System.Collections.Generic;
using System.Windows.Input;
using BudgetBadger.Forms.Style;
using BudgetBadger.Models;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class TransactionStatusButton : Grid
    {
        public static BindableProperty LabelProperty =
            BindableProperty.Create(nameof(Label),
                typeof(string),
                typeof(CurrencyCalculatorEntry),
                propertyChanged: (bindable, oldVal, newVal) =>
                {
                    if (bindable is TransactionStatusButton button && oldVal != newVal)
                    {
                        if (string.IsNullOrEmpty((string)newVal))
                        {
                            button.LabelControl.IsVisible = false;
                        }
                        else
                        {
                            button.LabelControl.IsVisible = true;
                        }
                    }
                });
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static BindableProperty TransactionProperty =
            BindableProperty.Create(nameof(Transaction),
                typeof(Transaction),
                typeof(TransactionStatusButton),
                null);
        public Transaction Transaction
        {
            get => (Transaction)GetValue(TransactionProperty);
            set => SetValue(TransactionProperty, value);
        }

        public static readonly BindableProperty ToggleCommandProperty =
            BindableProperty.Create(nameof(ToggleCommand),
                typeof(ICommand),
                typeof(TransactionStatusButton),
                null);
        public ICommand ToggleCommand
        {
            get { return (ICommand)GetValue(ToggleCommandProperty); }
            set { SetValue(ToggleCommandProperty, value); }
        }

        public static readonly BindableProperty HasCaptionProperty = BindableProperty.Create(nameof(HasCaption), typeof(bool), typeof(TransactionStatusButton));
        public bool HasCaption
        {
            get => (bool)GetValue(HasCaptionProperty);
            set => SetValue(HasCaptionProperty, value);
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

        private readonly bool _compact;

        public TransactionStatusButton() : this(false) { }

        public TransactionStatusButton(bool compact)
        {
            InitializeComponent();

            if (compact)
            {
                FrameControl.HeightRequest = (double)DynamicResourceProvider.Instance["size_175"];
                FrameControl.WidthRequest = (double)DynamicResourceProvider.Instance["size_175"];
                ReconciledImageControl.WidthRequest = (double)DynamicResourceProvider.Instance["size_175"];
                ReconciledImageControl.HeightRequest = (double)DynamicResourceProvider.Instance["size_175"];
                ReconciledImageSource.Size = (double)DynamicResourceProvider.Instance["size_175"];
                ImageControl.HeightRequest = (double)DynamicResourceProvider.Instance["size_85"];
                ImageControl.WidthRequest = (double)DynamicResourceProvider.Instance["size_85"];
                ImageControl.FontSize = (double)DynamicResourceProvider.Instance["size_65"];
                CaptionControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlLabelCompactStyle"];
                LabelControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlDescriptionLabelCompactStyle"];
                HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlHintLabelCompactStyle"];
            }
            else
            {
                FrameControl.HeightRequest = (double)DynamicResourceProvider.Instance["size_225"];
                FrameControl.WidthRequest = (double)DynamicResourceProvider.Instance["size_225"];
                ReconciledImageControl.WidthRequest = (double)DynamicResourceProvider.Instance["size_225"];
                ReconciledImageControl.HeightRequest = (double)DynamicResourceProvider.Instance["size_225"];
                ReconciledImageSource.Size = (double)DynamicResourceProvider.Instance["size_225"];
                ImageControl.HeightRequest = (double)DynamicResourceProvider.Instance["size_175"];
                ImageControl.WidthRequest = (double)DynamicResourceProvider.Instance["size_175"];
                ImageControl.FontSize = (double)DynamicResourceProvider.Instance["size_150"];
                CaptionControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlLabelStyle"];
                LabelControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlDescriptionLabelStyle"];
                HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlHintLabelStyle"];
            }

            _compact = compact;

            ButtonBackground.BindingContext = this;
            LabelControl.BindingContext = this;
            FrameControl.BindingContext = this;
            ImageControl.BindingContext = this;
            ReconciledImageControl.BindingContext = this;
            CaptionControl.BindingContext = this;
        }

        void Handle_Clicked(object sender, EventArgs e)
        {
            if (IsEnabled && Transaction.TransactionStatus != TransactionStatus.Reconciled)
            {
                if (ToggleCommand?.CanExecute(Transaction) ?? false)
                {
                    ToggleCommand?.Execute(Transaction);
                }
            }
        }

        static void UpdateErrorAndHint(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is TransactionStatusButton button && oldValue != newValue)
            {
                if (!String.IsNullOrEmpty(button.Error))
                {
                    button.HintErrorControl.IsVisible = true;
                    button.HintErrorControl.Text = button.Error;
                    if (button._compact)
                    {
                        button.HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlErrorLabelCompactStyle"];
                    }
                    else
                    {
                        button.HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlErrorLabelCompactStyle"];
                    }
                }
                else if (!String.IsNullOrEmpty(button.Hint))
                {
                    button.HintErrorControl.IsVisible = true;
                    button.HintErrorControl.Text = button.Hint;
                    if (button._compact)
                    {
                        button.HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlHintLabelCompactStyle"];
                    }
                    else
                    {
                        button.HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlHintLabelCompactStyle"];
                    }
                }
                else
                {
                    button.HintErrorControl.IsVisible = false;
                }
            }
        }
    }
}
