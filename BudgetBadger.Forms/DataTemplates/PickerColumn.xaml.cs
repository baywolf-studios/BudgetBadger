using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using BudgetBadger.Forms.UserControls;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class PickerColumn : ContentButton
    {
        ObservableCollection<object> internalItemsSource;

        public static BindableProperty IsReadOnlyProperty = BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(TextColumn));
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

        public static BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(PickerColumn), default(IList), propertyChanged: (bindable, oldVal, newVal) =>
        {
            ((PickerColumn)bindable).internalItemsSource.Clear();
            if (newVal != null && newVal is IList list)
            {
                foreach (var item in list)
                {
                    ((PickerColumn)bindable).internalItemsSource.Add(item);
                }
            }
        });
        public IList ItemsSource
        {
            get => (IList)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(PickerColumn), null, BindingMode.TwoWay, propertyChanged: (bindable, oldVal, newVal) =>
        {
            if (((PickerColumn)bindable).internalItemsSource != null 
                && !((PickerColumn)bindable).internalItemsSource.Contains(newVal)
                && newVal != null)
            {
                ((PickerColumn)bindable).internalItemsSource.Add(newVal);
            }

            var index = -1;
            if (((PickerColumn)bindable).PickerControl.ItemsSource != null)
            {
                index = ((PickerColumn)bindable).PickerControl.ItemsSource.IndexOf(newVal);
            }

            ((PickerColumn)bindable).PickerControl.SelectedIndex = index;
        });
        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static BindableProperty SaveCommandProperty = BindableProperty.Create(nameof(SaveCommand), typeof(ICommand), typeof(PickerColumn));
        public ICommand SaveCommand
        {
            get => (ICommand)GetValue(SaveCommandProperty);
            set => SetValue(SaveCommandProperty, value);
        }

        public static BindableProperty SaveCommandParameterProperty = BindableProperty.Create(nameof(SaveCommandParameter), typeof(object), typeof(PickerColumn));
        public object SaveCommandParameter
        {
            get => GetValue(SaveCommandParameterProperty);
            set => SetValue(SaveCommandParameterProperty, value);
        }

        public static BindableProperty SelectedCommandProperty = BindableProperty.Create(nameof(SelectedCommand), typeof(ICommand), typeof(TextColumn));
        public ICommand SelectedCommand
        {
            get => (ICommand)GetValue(SelectedCommandProperty);
            set => SetValue(SelectedCommandProperty, value);
        }

        public static BindableProperty SelectedCommandParameterProperty = BindableProperty.Create(nameof(SelectedCommandParameter), typeof(object), typeof(TextColumn));
        public object SelectedCommandParameter
        {
            get => GetValue(SelectedCommandParameterProperty);
            set => SetValue(SelectedCommandParameterProperty, value);
        }

        public PickerColumn()
        {
            InitializeComponent();
            internalItemsSource = new ObservableCollection<object>();
            PickerControl.BindingContext = this;
            LabelControl.BindingContext = this;
            PickerControl.ItemsSource = internalItemsSource;

            PickerControl.SelectedIndexChanged += (sender, e) =>
            {
                if (PickerControl.Items != null && PickerControl.SelectedIndex >= 0)
                {
                    LabelControl.Text = PickerControl.Items[PickerControl.SelectedIndex];
                }
                else
                {
                    LabelControl.Text = string.Empty;
                }

                if (!SelectedItem.Equals(PickerControl.SelectedItem))
                {
                    SelectedItem = PickerControl.SelectedItem;
                    if (SaveCommand != null && SaveCommand.CanExecute(SaveCommandParameter))
                    {
                        SaveCommand.Execute(SaveCommandParameter);
                    }
                }

                ForceActiveBackground = false;
                ForceActiveBackground = true;
            };
        }

        void Handle_Tapped(object sender, System.EventArgs e)
        {
            if (IsReadOnly || !IsEnabled)
            {
                if (SelectedCommand != null && SelectedCommand.CanExecute(SelectedCommandParameter))
                {
                    SelectedCommand.Execute(SelectedCommandParameter);
                }
                ForceActiveBackground = false;
                ForceActiveBackground = true;
            }
            else if (!PickerControl.IsFocused)
            {
                PickerControl.Focus();
            }
        }
    }
}