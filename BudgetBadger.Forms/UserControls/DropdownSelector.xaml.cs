using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Forms.Animation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class DropdownSelector : AbsoluteLayout
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

        public static BindableProperty LabelProperty =
            BindableProperty.Create(nameof(Label),
                                    typeof(string),
                                    typeof(DropdownSelector),
                                    defaultBindingMode: BindingMode.TwoWay);
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public BindingBase ItemDisplayBinding
        {
            get => PickerControl.ItemDisplayBinding;
            set => PickerControl.ItemDisplayBinding = value;
        }

        public static readonly BindableProperty SelectedIndexProperty =
            BindableProperty.Create(nameof(SelectedIndex),
                                    typeof(int),
                                    typeof(DropdownSelector),
                                    -1,
                                    BindingMode.TwoWay,
                                    propertyChanged: (bindable, oldVal, newVal) =>
        {
            if (oldVal != newVal)
            {
                ((DropdownSelector)bindable).PickerControl.SelectedIndex = (int)newVal;
            }
        });
        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource),
                                    typeof(IList),
                                    typeof(DropdownSelector),
                                    default(IList),
                                    propertyChanged: (bindable, oldVal, newVal) =>
                                    {
                                        if (oldVal != newVal)
                                        {
                                            ((DropdownSelector)bindable).PickerControl.ItemsSource = (IList)newVal;
                                        }
                                    });
        public IList ItemsSource
        {
            get => (IList)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem),
                                    typeof(object),
                                    typeof(DropdownSelector),
                                    null,
                                    BindingMode.TwoWay,
                                    propertyChanged: (bindable, oldVal, newVal) =>
                                    {
                                        if (oldVal != newVal)
                                        {
                                            ((DropdownSelector)bindable).PickerControl.SelectedItem = newVal;
                                        }
                                    });
        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public Dictionary<string, string> ReplaceColor
        {
            get
            {
                if (PickerControl.IsFocused)
                {
                    return new Dictionary<string, string> { { "currentColor", _focusedColor.GetHexString() } };
                }
                else if (IsEnabled)
                {
                    return new Dictionary<string, string> { { "currentColor", _idleColor.GetHexString() } };
                }
                else
                {
                    return new Dictionary<string, string> { { "currentColor", _disabledColor.GetHexString() } };
                }
            }
        }

        public event EventHandler<EventArgs> SelectedIndexChanged;

        public DropdownSelector()
        {
            InitializeComponent();
            LabelControl.BindingContext = this;
            IconControl.BindingContext = this;

            PickerControl.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(PickerControl.SelectedItem))
                {
                    SelectedItem = PickerControl.SelectedItem;
                }

                if (e.PropertyName == nameof(PickerControl.ItemsSource))
                {
                    ItemsSource = PickerControl.ItemsSource;
                }
            };
                

            PickerControl.SelectedIndexChanged += (sender, e) =>
            {
                SelectedIndex = PickerControl.SelectedIndex;
                SelectedIndexChanged?.Invoke(this, e);
                UpdateVisualState();
            };

            PickerControl.Focused += (sender, e) =>
            {
                UpdateVisualState();
            };

            PickerControl.Unfocused += (sender, e) =>
            {
                UpdateVisualState();
            };

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    UpdateVisualState();
                    if (Device.RuntimePlatform == Device.macOS)
                    {
                        PickerControl.IsEnabled = IsEnabled;
                    }
                }
            };

            UpdateVisualState();
        }

        async void UpdateVisualState()
        {
            if (PickerControl.IsFocused)
            {
                await SetFocusedVisualState();
            }
            else if (PickerControl.SelectedIndex < 0)
            {
                await SetIdleEmptyVisualState();
            }
            else
            {
                await SetIdleFilledVisualState();
            }
            OnPropertyChanged(nameof(ReplaceColor));
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
            tasks.Add(PickerControl.ColorTo(PickerControl.TextColor, textColor, c => PickerControl.TextColor = c, _animationLength, Easing.CubicInOut));

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
            tasks.Add(PickerControl.ColorTo(PickerControl.TextColor, textColor, c => PickerControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // color the label
            var labelColor = IsEnabled ? _idleColor : _disabledColor;
            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, labelColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // show the normal bottom border
            tasks.Add(BottomBorderControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            // hide the thick bottom border
            tasks.Add(ThickBottomBorderControl.FadeTo(0, _animationLength, Easing.CubicInOut));

            // move label upward
            var translationY = Device.RuntimePlatform == Device.macOS ? 24 : -24;
            tasks.Add(LabelControl.TranslateTo(0, translationY, _animationLength, Easing.CubicInOut));

            // shrink label text
            tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, 12, f => LabelControl.FontSize = f, _animationLength, Easing.CubicInOut));

            await Task.WhenAll(tasks);
        }

        async Task SetFocusedVisualState()
        {
            var tasks = new List<Task>();

            // color the bottom border
            tasks.Add(BottomBorderControl.ColorTo(BottomBorderControl.Color, _focusedColor, c => BottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            tasks.Add(ThickBottomBorderControl.ColorTo(ThickBottomBorderControl.Color, _focusedColor, c => ThickBottomBorderControl.Color = c, _animationLength, Easing.CubicInOut));

            // show the thick bottom border
            tasks.Add(ThickBottomBorderControl.FadeTo(1, _animationLength, Easing.CubicInOut));

            // hide the normal bottom border
            tasks.Add(BottomBorderControl.FadeTo(0, _animationLength, Easing.CubicInOut));

            // color the label
            tasks.Add(LabelControl.ColorTo(LabelControl.TextColor, _focusedColor, c => LabelControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // color the picker
            tasks.Add(PickerControl.ColorTo(PickerControl.TextColor, _textColor, c => PickerControl.TextColor = c, _animationLength, Easing.CubicInOut));

            // move upward
            var translationY = Device.RuntimePlatform == Device.macOS ? 24 : -24;
            tasks.Add(LabelControl.TranslateTo(0, translationY, _animationLength, Easing.CubicInOut));

            // shrink font size
            tasks.Add(LabelControl.DoubleTo(LabelControl.FontSize, 12, f => LabelControl.FontSize = f, _animationLength, Easing.CubicInOut));

            await Task.WhenAll(tasks);
        }

        void Handle_Tapped(object sender, System.EventArgs e)
        {
            if (!PickerControl.IsFocused)
            {
                PickerControl.Focus();
            }
        }
    }
}

