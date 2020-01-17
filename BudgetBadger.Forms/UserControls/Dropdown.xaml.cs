using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Forms.Animation;
using BudgetBadger.Forms.Effects;
using BudgetBadger.Forms.Style;
using BudgetBadger.Models;
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

        public BindingBase ItemDisplayBinding
        {
            get => PickerControl.ItemDisplayBinding;
            set => PickerControl.ItemDisplayBinding = value;
        }

        public static BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(Dropdown), default(IList), propertyChanged: (bindable, oldVal, newVal) =>
        {
            if (bindable is Dropdown dropdown)
            {
                var items = dropdown.PickerControl.ItemsSource as ObservableList<object>;

                var currentSelectedItem = dropdown.SelectedItem;

                if (newVal != null)
                {
                    items.ReplaceRange(((IList)newVal).Cast<object>());
                }
                else
                {
                    items.Clear();
                }

                dropdown.PickerControl.SelectedItem = currentSelectedItem;
            }
        });
        public IList ItemsSource
        {
            get => (IList)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(Dropdown), -1, BindingMode.TwoWay, propertyChanged: (bindable, oldVal, newVal) =>
        {
            if (bindable is Dropdown dropdown)
            {
                dropdown.PickerControl.SelectedIndex = (int)newVal;
                if ((int)newVal >= 0 && dropdown.PickerControl.Items.Count > (int)newVal)
                {
                    dropdown.ReadOnlyPickerControl.Text = dropdown.PickerControl.Items[(int)newVal];
                }
                else
                {
                    dropdown.ReadOnlyPickerControl.Text = string.Empty;
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
            if (bindable is Dropdown dropdown)
            {
                if (dropdown.PickerControl.ItemsSource != null
                    && !(dropdown.PickerControl.ItemsSource.Contains(newVal))
                    && newVal != null)
                {
                    dropdown.PickerControl.ItemsSource.Add(newVal);

                }

                if (dropdown.PickerControl.ItemsSource != null
                    && dropdown.PickerControl.ItemsSource.Contains(oldVal)
                    && !dropdown.ItemsSource.Contains(oldVal))
                {
                    dropdown.PickerControl.ItemsSource.Remove(oldVal);
                }

                var index = -1;
                if (dropdown.PickerControl.ItemsSource != null)
                {
                    index = dropdown.PickerControl.ItemsSource.IndexOf(newVal);
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

            PickerControl.ItemsSource = new ObservableList<object>();

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
                SelectedItem = PickerControl.SelectedItem;
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

