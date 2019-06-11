﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class PickerColumn : ContentView
    {
        public BindingBase ItemDisplayBinding
        {
            get => PickerControl.ItemDisplayBinding;
            set => PickerControl.ItemDisplayBinding = value;
        }

        public static BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(PickerColumn), default(IList), propertyChanged: (bindable, oldVal, newVal) =>
        {
            ((PickerColumn)bindable).PickerControl.ItemsSource = (IList)newVal;
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
            if (((PickerColumn)bindable).PickerControl.ItemsSource != null 
                && !((PickerColumn)bindable).PickerControl.ItemsSource.Contains(newVal))
            {
                ((PickerColumn)bindable).PickerControl.ItemsSource.Add(newVal);
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
            PickerControl.BindingContext = this;

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    PickerControl.IsEnabled = IsEnabled;
                }
            };

            PickerControl.Unfocused += (sender, e) =>
            {
                BackgroundColor = Color.Transparent;

                if (SaveCommand != null && SaveCommand.CanExecute(SaveCommandParameter))
                {
                    SaveCommand.Execute(SaveCommandParameter);
                }
            };

            PickerControl.Focused += (sender, e) =>
            {
                BackgroundColor = (Color)Application.Current.Resources["SelectedItemColor"];
            };

            PickerControl.SelectedIndexChanged += (sender, e) =>
            {
                var index = SelectedIndex;
                var item = SelectedItem;
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