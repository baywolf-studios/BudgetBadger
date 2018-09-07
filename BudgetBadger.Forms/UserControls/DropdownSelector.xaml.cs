using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBadger.Forms.Animation;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class DropdownSelector : StackLayout
    {
        public static BindableProperty LabelProperty =
            BindableProperty.Create(nameof(Label),
                                    typeof(string),
                                    typeof(DropdownSelector));
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

        public static BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(DropdownSelector), -1, BindingMode.TwoWay);
        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(DropdownSelector), null, BindingMode.TwoWay);
        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(DropdownSelector), null);
        public IList ItemsSource
        {
            get => (IList)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public event EventHandler<EventArgs> SelectedIndexChanged;

        public DropdownSelector()
        {
            InitializeComponent();
            LabelControl.BindingContext = this;
            PickerControl.BindingContext = this;

            PickerControl.SelectedIndexChanged += (sender, e) =>
            {
                SelectedIndexChanged?.Invoke(this, e);
                if (SelectedItem != PickerControl.SelectedItem)
                {
                    SelectedItem = PickerControl.SelectedItem;
                }
                if (SelectedIndex != PickerControl.SelectedIndex)
                {
                    SelectedIndex = PickerControl.SelectedIndex;
                }
            };

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    PickerControl.IsEnabled = IsEnabled;
                }
            };
        }
    }
}

