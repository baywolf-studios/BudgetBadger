using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Forms.UserControls;
using Xamarin.Forms;

namespace BudgetBadger.Forms.DataTemplates
{
    public partial class DatePickerColumn : ContentButton
    {
        readonly IResourceContainer _resourceContainer;
        readonly ILocalize _localize;

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
                                        ((DatePickerColumn)bindable).TextControl.Text = ((DatePickerColumn)bindable)._resourceContainer.GetFormattedString("{0:d}", newVal);
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
                if (Application.Current.Resources.TryGetValue("DenseDataTableDatePickerColumnCellStyle", out object resource))
                {
                    DateControl.Style = (Xamarin.Forms.Style)resource;
                }

                if (Application.Current.Resources.TryGetValue("DenseDataTableTextColumnCellStyle", out object resource2))
                {
                    TextControl.Style = (Xamarin.Forms.Style)resource2;
                }
            }
            else
            {
                if (Application.Current.Resources.TryGetValue("DataTableDatePickerColumnCellStyle", out object resource))
                {
                    DateControl.Style = (Xamarin.Forms.Style)resource;
                }

                if (Application.Current.Resources.TryGetValue("DataTableTextColumnCellStyle", out object resource2))
                {
                    TextControl.Style = (Xamarin.Forms.Style)resource2;
                }
            }

            _resourceContainer = StaticResourceContainer.Current;
            _localize = DependencyService.Get<ILocalize>();
            DateControl.BindingContext = this;
            TextControl.BindingContext = this;

            DateControl.Focused += Control_Focused;
            TextControl.Focused += Control_Focused;

            DateControl.Unfocused += DateControl_DateSelected;
            DateControl.DateSelected += DateControl_DateSelected;

            TextControl.Unfocused += TextControl_Completed;
            TextControl.Completed += TextControl_Completed;
        }

        private void TextControl_Completed(object sender, EventArgs e)
        {
            var locale = _localize.GetLocale() ?? CultureInfo.CurrentUICulture;
            var dfi = locale.DateTimeFormat;

            if (DateTime.TryParse(TextControl.Text, dfi, DateTimeStyles.AllowWhiteSpaces, out DateTime result))
            {
                if (!Date.Date.Equals(result.Date))
                {
                    Date = result.Date;
                    if (SaveCommand != null && SaveCommand.CanExecute(SaveCommandParameter))
                    {
                        SaveCommand.Execute(SaveCommandParameter);
                    }
                }
            }
            else
            {
                TextControl.Text = _resourceContainer.GetFormattedString("{0:d}", Date);
            }

            ForceActiveBackground = false;
            ForceActiveBackground = true;
        }

        private void DateControl_DateSelected(object sender, EventArgs e)
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
        }

        void Control_Focused(object sender, FocusEventArgs e)
        {
            if (IsReadOnly || !IsEnabled)
            {
                DateControl.Unfocus();
                TextControl.Unfocus();
            }

            UpdateActive();
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
            else
            {
                DateControl.Focus();
            }
        }
    }
}
