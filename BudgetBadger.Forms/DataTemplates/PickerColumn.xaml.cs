using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BudgetBadger.Forms.UserControls;
using BudgetBadger.Models;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class PickerColumn : ContentButton
    {
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
            var items = ((PickerColumn)bindable).PickerControl.ItemsSource as ObservableRangeCollection<object>;
            if (newVal != null)
            {
                items.ReplaceRange(newVal as IEnumerable<object>);
            }
            else
            {
                items.Clear();
            }
        });
        public IList ItemsSource
        {
            get => (IList)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(PickerColumn), null, BindingMode.TwoWay, propertyChanged: (bindable, oldVal, newVal) =>
        {
            if (((PickerColumn)bindable).PickerControl.ItemsSource != null 
                && !((PickerColumn)bindable).PickerControl.ItemsSource.Contains(newVal)
                && newVal != null)
            {
                ((PickerColumn)bindable).PickerControl.ItemsSource.Add(newVal);
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
            PickerControl.BindingContext = this;
            LabelControl.BindingContext = this;
            PickerControl.ItemsSource = new ObservableRangeCollection<object>();

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

                if (!(SelectedItem == null && PickerControl.SelectedItem == null)
                && PickerControl.SelectedItem != null)
                {
                    if ((SelectedItem == null && PickerControl.SelectedItem != null)
                        || (SelectedItem != null && PickerControl.SelectedItem == null)
                        || !(SelectedItem.Equals(PickerControl.SelectedItem)))
                    {
                        SelectedItem = PickerControl.SelectedItem;
                        if (SaveCommand != null && SaveCommand.CanExecute(SaveCommandParameter))
                        {
                            SaveCommand.Execute(SaveCommandParameter);
                        }
                    }
                }

                ForceActiveBackground = false;
                ForceActiveBackground = true;
            };

            PickerControl.Focused += (sender, e) =>
            {
                UpdateActive();
            };

            PickerControl.Unfocused += (sender, e) =>
            {
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