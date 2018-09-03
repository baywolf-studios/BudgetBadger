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

        public static readonly BindableProperty SelectedIndexProperty =
            BindableProperty.Create(nameof(SelectedIndex),
                                    typeof(int),
                                    typeof(DropdownSelector),
                                    -1);
        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource),
                                    typeof(IList),
                                    typeof(DropdownSelector),
                                    default(IList));
        public IList ItemsSource
        {
            get => (IList)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem),
                                    typeof(object),
                                    typeof(DropdownSelector),
                                    null);
        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
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

