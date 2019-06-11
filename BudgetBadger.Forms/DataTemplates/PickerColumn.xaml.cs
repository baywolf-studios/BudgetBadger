using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class PickerColumn : ContentView
    {
        ObservableCollection<object> internalItemsSource;

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

        public static BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(PickerColumn), -1, BindingMode.TwoWay);
        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(PickerColumn), null, BindingMode.TwoWay, propertyChanged: (bindable, oldVal, newVal) =>
        {
            if (((PickerColumn)bindable).internalItemsSource != null 
                && !((PickerColumn)bindable).internalItemsSource.Contains(newVal)
                && newVal != null)
            {
                ((PickerColumn)bindable).internalItemsSource.Add(newVal);
            }
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

        public PickerColumn()
        {
            InitializeComponent();
            internalItemsSource = new ObservableCollection<object>();
            PickerControl.BindingContext = this;
            PickerControl.ItemsSource = internalItemsSource;

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    PickerControl.IsEnabled = IsEnabled;
                }
            };

            PickerControl.Focused += (sender, e) =>
            {
                BackgroundColor = (Color)Application.Current.Resources["SelectedItemColor"];
            };

            PickerControl.SelectedIndexChanged += (sender, e) =>
            {
                BackgroundColor = Color.Transparent;

                if (SaveCommand != null && SaveCommand.CanExecute(SaveCommandParameter))
                {
                    SaveCommand.Execute(SaveCommandParameter);
                }
            };
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