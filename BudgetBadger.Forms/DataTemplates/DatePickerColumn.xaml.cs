using System;
using System.Collections.Generic;
using System.Windows.Input;
using BudgetBadger.Forms.UserControls;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class DatePickerColumn : ContentButton
    {
        public static BindableProperty IsReadOnlyProperty = BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(TextColumn));
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public static BindableProperty DateProperty =
            BindableProperty.Create(nameof(Date),
                                    typeof(DateTime),
                                    typeof(DatePickerColumn),
                                    defaultValue: DateTime.Now,
                                    defaultBindingMode: BindingMode.TwoWay,
                                    propertyChanged: (bindable, oldVal, newVal) =>
                                    {
                                        ((DatePickerColumn)bindable).DateControl.Date = ((DateTime)newVal).Date;
                                    });
        public DateTime Date
        {
            get => (DateTime)GetValue(DateProperty);
            set => SetValue(DateProperty, value);
        }

        public static BindableProperty SaveCommandProperty = BindableProperty.Create(nameof(SaveCommand), typeof(ICommand), typeof(DatePickerColumn));
        public ICommand SaveCommand
        {
            get => (ICommand)GetValue(SaveCommandProperty);
            set => SetValue(SaveCommandProperty, value);
        }

        public static BindableProperty SaveCommandParameterProperty = BindableProperty.Create(nameof(SaveCommandParameter), typeof(object), typeof(DatePickerColumn));
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

        public DatePickerColumn() : this(false) { }

        public DatePickerColumn(bool dense)
        {
            InitializeComponent();
            if (dense)
            {
                DateControl.FontSize = (double)Application.Current.Resources["DataGridItemDenseFontSize"];
            }
            else
            {
                DateControl.FontSize = (double)Application.Current.Resources["DataGridItemFontSize"];
            }

            DateControl.BindingContext = this;

            DateControl.Focused += (sender, e) =>
            {
                if (IsReadOnly || !IsEnabled)
                {
                    DateControl.Unfocus();
                }

                UpdateActive();
            };

            DateControl.Unfocused += (sender, e) =>
            {
                ForceActiveBackground = false;
                ForceActiveBackground = true;
            };

            DateControl.DateSelected += (sender, e) =>
            {
                if (!Date.Date.Equals(DateControl.Date.Date))
                {
                    Date = DateControl.Date.Date;
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
            else if (!DateControl.IsFocused)
            {
                DateControl.Focus();
            }
        }
    }
}
