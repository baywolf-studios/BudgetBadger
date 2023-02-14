using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BudgetBadger.Forms.Animation;
using BudgetBadger.Forms.Effects;
using BudgetBadger.Forms.Style;
using BudgetBadger.Core.Models;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class Dropdown : Grid
    {
        public static BindableProperty LabelProperty =
            BindableProperty.Create(nameof(Label),
                typeof(string),
                typeof(TextField),
                propertyChanged: (bindable, oldVal, newVal) =>
                {
                    if (bindable is Dropdown dropdown && oldVal != newVal)
                    {
                        if (string.IsNullOrEmpty((string)newVal))
                        {
                            dropdown.LabelControl.IsVisible = false;
                        }
                        else
                        {
                            dropdown.LabelControl.IsVisible = true;
                        }
                    }
                });
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
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

        public static BindableProperty IsReadOnlyProperty = BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(Dropdown));
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public static BindableProperty ItemPropertyDescriptionProperty = BindableProperty.Create(nameof(ItemPropertyDescription), typeof(string), typeof(Dropdown));
        public string ItemPropertyDescription
        {
            get => (string)GetValue(ItemPropertyDescriptionProperty);
            set => SetValue(ItemPropertyDescriptionProperty, value);
        }

        private static string GetPropertyValue(object src, string propertyName)
        {
            if (src == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                return src.ToString();
            }

            var propertyValue = src.GetType().GetRuntimeProperty(propertyName)?.GetValue(src);
            return propertyValue == null ? string.Empty : propertyValue.ToString();
        }

        public static BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(Dropdown), default(IList), propertyChanged: (bindable, oldVal, newVal) =>
        {
            if (bindable is Dropdown dropdown && oldVal != newVal)
            {
                var items2 = dropdown.PickerControl.Items;
                items2.Clear();
                if (newVal != null)
                {
                    foreach (var i in (IList)newVal)
                    {
                        items2.Add(GetPropertyValue(i, dropdown.ItemPropertyDescription));
                    }
                }
            }
        });
        public IList ItemsSource
        {
            get => (IList)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(Dropdown), -1, BindingMode.TwoWay, propertyChanged: (bindable, oldVal, newVal) =>
        {
            if (bindable is Dropdown dropdown && oldVal != newVal && newVal is int intNewVal)
            {
                dropdown.PickerControl.SelectedIndex = intNewVal;

                if (intNewVal >= 0 && dropdown.ItemsSource != null)
                {
                    dropdown.SelectedItem = dropdown.ItemsSource[intNewVal];
                }
            }
        });
        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(Dropdown), null, BindingMode.TwoWay, propertyChanged: (bindable, oldVal, newVal) =>
        {
            if (bindable is Dropdown dropdown && oldVal != newVal)
            {
                var index = -1;
                var stringItem = GetPropertyValue(newVal, dropdown.ItemPropertyDescription);

                dropdown.ReadOnlyPickerControl.Text = stringItem;

                if (dropdown.PickerControl.Items != null)
                {
                    index = dropdown.PickerControl.Items.IndexOf(stringItem);
                }

                dropdown.SelectedIndex = index;
            }
        });
        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public event EventHandler<EventArgs> SelectedIndexChanged;

        private readonly bool _compact;

        public Dropdown() : this(false) { }

        public Dropdown(bool compact)
        {
            InitializeComponent();

            if (compact)
            {
                PickerControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlPickerCompactStyle"];
                ReadOnlyPickerControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlLabelCompactStyle"];
                LabelControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlDescriptionLabelCompactStyle"];
                HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlHintLabelCompactStyle"];
            }
            else
            {
                PickerControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlPickerStyle"];
                ReadOnlyPickerControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlLabelStyle"];
                LabelControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlDescriptionLabelStyle"];
                HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlHintLabelStyle"];
            }

            ButtonBackground.BindingContext = this;
            LabelControl.BindingContext = this;
            PickerControl.BindingContext = this;
            ReadOnlyPickerControl.BindingContext = this;

            PickerControl.SelectedIndexChanged += PickerControl_SelectedIndexChanged;
            PickerControl.Focused += Control_Focused;

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    PickerControl.IsEnabled = IsEnabled;
                }
            };
        }

        void Control_Focused(object sender, FocusEventArgs e)
        {
            if (IsReadOnly || !IsEnabled)
            {
                PickerControl.Unfocus();
                LabelControl.Unfocus();
            }
        }

        void PickerControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedIndex != PickerControl.SelectedIndex)
            { 
                SelectedIndex = PickerControl.SelectedIndex;
                SelectedIndexChanged?.Invoke(this, e);
            }
        }

        void Handle_Clicked(object sender, EventArgs e)
        {
            if (!IsReadOnly && IsEnabled && !PickerControl.IsFocused)
            {
                PickerControl.Focus();
            }
        }

        static void UpdateErrorAndHint(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is Dropdown dropdown && oldValue != newValue)
            {
                if (!String.IsNullOrEmpty(dropdown.Error))
                {
                    dropdown.HintErrorControl.IsVisible = true;
                    dropdown.HintErrorControl.Text = dropdown.Error;
                    if (dropdown._compact)
                    {
                        dropdown.HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlErrorLabelCompactStyle"];
                    }
                    else
                    {
                        dropdown.HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlErrorLabelCompactStyle"];
                    }
                }
                else if (!String.IsNullOrEmpty(dropdown.Hint))
                {
                    dropdown.HintErrorControl.IsVisible = true;
                    dropdown.HintErrorControl.Text = dropdown.Hint;
                    if (dropdown._compact)
                    {
                        dropdown.HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlHintLabelCompactStyle"];
                    }
                    else
                    {
                        dropdown.HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlHintLabelCompactStyle"];
                    }
                }
                else
                {
                    dropdown.HintErrorControl.IsVisible = false;
                }
            }
        }
    }
}

