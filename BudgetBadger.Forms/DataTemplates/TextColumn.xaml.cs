﻿using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class TextColumn : ContentView
    {
        public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(TextColumn), defaultBindingMode: BindingMode.TwoWay);
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static BindableProperty SaveCommandProperty = BindableProperty.Create(nameof(SaveCommand), typeof(ICommand), typeof(TextColumn));
        public ICommand SaveCommand
        {
            get => (ICommand)GetValue(SaveCommandProperty);
            set => SetValue(SaveCommandProperty, value);
        }

        public static BindableProperty SaveCommandParameterProperty = BindableProperty.Create(nameof(SaveCommandParameter), typeof(object), typeof(TextColumn));
        public object SaveCommandParameter
        {
            get => GetValue(SaveCommandParameterProperty);
            set => SetValue(SaveCommandParameterProperty, value);
        }

        public TextColumn()
        {
            InitializeComponent();
            TextControl.BindingContext = this;

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    TextControl.IsEnabled = IsEnabled;
                }
            };

            TextControl.Unfocused += (sender, e) =>
            {
                if (SaveCommand != null && SaveCommand.CanExecute(SaveCommandParameter))
                {
                    SaveCommand.Execute(SaveCommandParameter);
                }
            };


        }
    }
}