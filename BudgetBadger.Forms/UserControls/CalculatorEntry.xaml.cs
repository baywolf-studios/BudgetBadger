using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using BudgetBadger.Forms.Animation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class CalculatorEntry : AbsoluteLayout
    {
        uint _animationLength = 150;

        Color _disabledColor
        {
            get => (Color)Application.Current.Resources["DisabledColor"];
        }

        Color _errorColor
        {
            get => (Color)Application.Current.Resources["ErrorColor"];
        }

        Color _idleColor
        {
            get => (Color)Application.Current.Resources["IdleColor"];
        }

        Color _focusedColor
        {
            get => (Color)Application.Current.Resources["PrimaryColor"];
        }

        Color _textColor
        {
            get => (Color)Application.Current.Resources["PrimaryTextColor"];
        }

        public static BindableProperty PrefixProperty = BindableProperty.Create(nameof(Prefix), typeof(string), typeof(TextEntry), defaultBindingMode: BindingMode.TwoWay);
        public string Prefix
        {
            get => (string)GetValue(PrefixProperty);
            set => SetValue(PrefixProperty, value);
        }

        public static BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(CalculatorEntry), defaultBindingMode: BindingMode.TwoWay);
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static BindableProperty NumberProperty = BindableProperty.Create(nameof(Number), typeof(decimal?), typeof(CalculatorEntry), defaultBindingMode: BindingMode.TwoWay);
        public decimal? Number
        {
            get => (decimal?)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }

        public CalculatorEntry()
        {
            InitializeComponent();

            LabelControl.BindingContext = this;
            PrefixControl.BindingContext = this;

            TextControl.Focused += (sender, e) =>
            {
                //EntryFocused?.Invoke(this, e);
                UpdateVisualState();
            };

            TextControl.Unfocused += (sender, e) =>
            {
                //EntryUnfocused?.Invoke(this, e);
                if (string.IsNullOrEmpty(TextControl.Text))
                {
                    Number = null;
                }
                else
                {
                    if (!decimal.TryParse(TextControl.Text, out decimal result))
                    {
                        try
                        {
                            var nfi = CultureInfo.CurrentCulture.NumberFormat;
                            var groupSeparator = nfi.CurrencyGroupSeparator;
                            var decimalSeparator = nfi.CurrencyDecimalSeparator;
                            var text = TextControl.Text.Replace(groupSeparator, "").Replace(decimalSeparator, ".").Replace("(", "-").Replace(")", "");
                            var temp = new DataTable().Compute(text, null);
                            result = Convert.ToDecimal(temp);
                        }
                        catch (Exception ex)
                        {
                            result = 0;
                        }
                    }
                    Number = result;
                }

                OnPropertyChanged(nameof(Number));

                UpdateVisualState();
            };

            TextControl.TextChanged += (sender, e) =>
            {
                //TextChanged?.Invoke(sender, e);
                UpdateVisualState();
            };

            PrefixControl.SizeChanged += (sender, e) =>
            {
                UpdateVisualState();
            };

            TextControl.Completed += (sender, e) =>
            {
                if (string.IsNullOrEmpty(TextControl.Text))
                {
                    Number = null;
                }
                else
                {
                    if (!decimal.TryParse(TextControl.Text, out decimal result))
                    {
                        try
                        {
                            var nfi = CultureInfo.CurrentCulture.NumberFormat;
                            var groupSeparator = nfi.CurrencyGroupSeparator;
                            var decimalSeparator = nfi.CurrencyDecimalSeparator;
                            var text = TextControl.Text.Replace(groupSeparator, "").Replace(decimalSeparator, ".").Replace("(", "-").Replace(")", "");
                            var temp = new DataTable().Compute(text, null);
                            result = Convert.ToDecimal(temp);
                        }
                        catch (Exception ex)
                        {
                            result = 0;
                        }
                    }
                    Number = Convert.ToDecimal(result);
                }

                OnPropertyChanged(nameof(Number));
            };

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    UpdateVisualState();
                }

                if (e.PropertyName == nameof(Number))
                {
                    NumberFormatInfo nfi = CultureInfo.CurrentCulture.NumberFormat;
                    nfi = (NumberFormatInfo)nfi.Clone();
                    nfi.CurrencySymbol = "";
                    TextControl.Text = Number.HasValue ? Number.Value.ToString("C", nfi) : string.Empty;
                }
            };

            if (Device.RuntimePlatform == Device.macOS)
            {
                TextControl.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(TextControl.IsFocused))
                    {
                        UpdateVisualState();
                    }
                };

                TextControl.Completed += (sender, e) =>
                {
                    UpdateVisualState();
                };
            }

            UpdateVisualState();
        }

        async void UpdateVisualState()
        {
            if (TextControl.IsFocused)
            {
                await SetFocusedVisualState();
            }
            else if (string.IsNullOrEmpty(TextControl.Text))
            {
                await SetIdleEmptyVisualState();
            }
            else
            {
                await SetIdleFilledVisualState();
            }
        }

        async Task SetIdleEmptyVisualState()
        {
            var tasks = new List<Task>();

            // color the bottom border
            var bottomBorderColor = IsEnabled ? _idleColor : _disabledColor;
            tasks.Add(BottomBorderControl.ColorTo(BottomBorderControl.Color, bottomBorderColor, c => BottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.ColorTo(ThickBottomBorderControl.Color, bottomBorderColor, c => ThickBottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            // color the text
            var textColor = IsEnabled ? _textColor : _disabledColor;
            tasks.Add(TextControl.ColorTo(TextControl.TextColor, textColor, c => TextControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // color the label
            var labelColor = IsEnabled ? _idleColor : _disabledColor;
            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, labelColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // show the normal bottom border
            tasks.Add(BottomBorderControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            // hide the thick bottom border
            tasks.Add(ThickBottomBorderControl.FadeTo(0, _animationLength, Easing.CubicInOut));

            // reset to original position
            tasks.Add(LabelControl.TranslateTo(0, 0, _animationLength, Easing.CubicInOut));

            // reset to original font size
            tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, 16, f => LabelControl.FontSize = f, _animationLength, Easing.CubicInOut));

            // hide the prefix
            tasks.Add(PrefixControl.FadeTo(0, _animationLength, Easing.CubicInOut));

            await Task.WhenAll(tasks);
        }

        async Task SetIdleFilledVisualState()
        {
            var tasks = new List<Task>();

            // color the bottom border
            var bottomBorderColor = IsEnabled ? _idleColor : _disabledColor;
            tasks.Add(BottomBorderControl.ColorTo(BottomBorderControl.Color, bottomBorderColor, c => BottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.ColorTo(ThickBottomBorderControl.Color, bottomBorderColor, c => ThickBottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            // color the text
            var textColor = IsEnabled ? _textColor : _disabledColor;
            tasks.Add(TextControl.ColorTo(TextControl.TextColor, textColor, c => TextControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // color the label
            var labelColor = IsEnabled ? _idleColor : _disabledColor;
            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, labelColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // color the prefix
            var prefixColor = IsEnabled ? _idleColor : _disabledColor;
            tasks.Add(PrefixControl.ColorTo(PrefixControl.TextColor, prefixColor, c => PrefixControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // show the normal bottom border
            tasks.Add(BottomBorderControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            // hide the thick bottom border
            tasks.Add(ThickBottomBorderControl.FadeTo(0, _animationLength, Easing.CubicInOut));

            // move label upward
            var translationY = Device.RuntimePlatform == Device.macOS ? 24 : -24;
            tasks.Add(LabelControl.TranslateTo(0, translationY, _animationLength, Easing.CubicInOut));

            // shrink label text
            tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, 12, f => LabelControl.FontSize = f, _animationLength, Easing.CubicInOut));

            // show the prefix
            tasks.Add(PrefixControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            //move entry over
            var entryLeftMargin = string.IsNullOrEmpty(Prefix) ? 0 : PrefixControl.Width + 4;
            var entryMargin = new Thickness(entryLeftMargin, 0, 0, 0);
            tasks.Add(TextControl.ThicknessTo(TextControl.Margin, entryMargin, m => TextControl.Margin = m, _animationLength, Easing.CubicInOut));

            await Task.WhenAll(tasks);
        }

        async Task SetFocusedVisualState()
        {
            var tasks = new List<Task>();

            // color bottom border
            tasks.Add(BottomBorderControl.ColorTo(BottomBorderControl.Color, _focusedColor, c => BottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.ColorTo(ThickBottomBorderControl.Color, _focusedColor, c => ThickBottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            // color the label control
            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, _focusedColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // color the text control
            tasks.Add(TextControl.ColorTo(TextControl.TextColor, _textColor, c => TextControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // color the prefix
            tasks.Add(PrefixControl.ColorTo(PrefixControl.TextColor, _idleColor, c => PrefixControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // show the thick bottom border
            tasks.Add(ThickBottomBorderControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            // hide the normal bottom border
            tasks.Add(BottomBorderControl.FadeTo(0, _animationLength, Easing.CubicInOut));

            // move upward
            var translationY = Device.RuntimePlatform == Device.macOS ? 24 : -24;
            tasks.Add(LabelControl.TranslateTo(0, translationY, _animationLength, Easing.CubicInOut));

            // shrink font size
            tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, 12, f => LabelControl.FontSize = f, _animationLength, Easing.CubicInOut));

            // show the prefix
            tasks.Add(PrefixControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            //move entry over
            var entryLeftMargin = string.IsNullOrEmpty(Prefix) ? 0 : PrefixControl.Width + 4;
            var entryMargin = new Thickness(entryLeftMargin, 0, 0, 0);
            tasks.Add(TextControl.ThicknessTo(TextControl.Margin, entryMargin, m => TextControl.Margin = m, _animationLength, Easing.CubicInOut));

            await Task.WhenAll(tasks);
        }

        void Handle_Tapped(object sender, System.EventArgs e)
        {
            if (!TextControl.IsFocused)
            {
                TextControl.Focus();
            }
        }
    }
}
