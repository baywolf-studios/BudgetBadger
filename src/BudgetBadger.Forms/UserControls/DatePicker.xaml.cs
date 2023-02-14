using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using BudgetBadger.Core.Localization;
using BudgetBadger.Forms.Animation;
using BudgetBadger.Forms.Style;
using Xamarin.Forms;

namespace BudgetBadger.Forms.UserControls
{
    public partial class DatePicker : Grid
    {
        readonly IResourceContainer _resourceContainer;
        readonly ILocalize _localize;

        public static BindableProperty LabelProperty =
            BindableProperty.Create(nameof(Label),
                typeof(string),
                typeof(DatePicker),
                propertyChanged: (bindable, oldVal, newVal) =>
                {
                    if (bindable is DatePicker datePicker && oldVal != newVal)
                    {
                        if (string.IsNullOrEmpty((string)newVal))
                        {
                            datePicker.LabelControl.IsVisible = false;
                        }
                        else
                        {
                            datePicker.LabelControl.IsVisible = true;
                        }
                    }
                });
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static BindableProperty HintProperty = BindableProperty.Create(nameof(Hint), typeof(string), typeof(DatePicker), propertyChanged: UpdateErrorAndHint);
        public string Hint
        {
            get => (string)GetValue(HintProperty);
            set => SetValue(HintProperty, value);
        }

        public static BindableProperty ErrorProperty = BindableProperty.Create(nameof(Error), typeof(string), typeof(DatePicker), propertyChanged: UpdateErrorAndHint);
        public string Error
        {
            get => (string)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }

        public static BindableProperty IsReadOnlyProperty = BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(DatePicker));
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public static BindableProperty UseTextFieldProperty = BindableProperty.Create(nameof(UseTextField), typeof(bool), typeof(DatePicker));
        public bool UseTextField
        {
            get => (bool)GetValue(UseTextFieldProperty);
            set => SetValue(UseTextFieldProperty, value);
        }

        public static BindableProperty DateProperty =
            BindableProperty.Create(nameof(Date),
                typeof(DateTime),
                typeof(DatePicker),
                defaultValue: DateTime.Now,
                defaultBindingMode: BindingMode.TwoWay,
                propertyChanged: (bindable, oldVal, newVal) =>
                {
                    if (bindable is DatePicker datePicker && oldVal != newVal)
                    {
                        datePicker.DateControl.Date = ((DateTime)newVal).Date;
                        datePicker.TextControl.Text = datePicker._resourceContainer.GetFormattedString("{0:d}", newVal);
                        datePicker.ReadOnlyDatePickerControl.Text = datePicker._resourceContainer.GetFormattedString("{0:d}", newVal);
                    }
                });
        public DateTime Date
        {
            get => (DateTime)GetValue(DateProperty);
            set => SetValue(DateProperty, value);
        }

        public event EventHandler<DateChangedEventArgs> DateSelected;

        private readonly bool _compact;

        public DatePicker() : this(false) { }

        public DatePicker(bool compact)
        {
            InitializeComponent();

            if (compact)
            {
                DateControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlDatePickerCompactStyle"];
                TextControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlEntryCompactStyle"];
                ReadOnlyDatePickerControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlLabelCompactStyle"];
                LabelControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlDescriptionLabelCompactStyle"];
                HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlHintLabelCompactStyle"];
            }
            else
            {
                DateControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlDatePickerStyle"];
                TextControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlEntryStyle"];
                ReadOnlyDatePickerControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlLabelStyle"];
                LabelControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlDescriptionLabelStyle"];
                HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlHintLabelStyle"];
            }
            _compact = compact;

            _resourceContainer = StaticResourceContainer.Current;
            _localize = DependencyService.Get<ILocalize>();

            ButtonBackground.BindingContext = this;
            LabelControl.BindingContext = this;
            DateControl.BindingContext = this;
            ReadOnlyDatePickerControl.BindingContext = this;
            TextControl.BindingContext = this;

            DateControl.Focused += Control_Focused;
            TextControl.Focused += Control_Focused;

            DateControl.Unfocused += DateControl_DateSelected;
            DateControl.DateSelected += DateControl_DateSelected;

            TextControl.Unfocused += TextControl_Completed;
            TextControl.Completed += TextControl_Completed;

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IsEnabled))
                {
                    DateControl.IsEnabled = IsEnabled;
                }
            };

            TextControl.Text = _resourceContainer.GetFormattedString("{0:d}", Date);
            ReadOnlyDatePickerControl.Text = _resourceContainer.GetFormattedString("{0:d}", Date);
        }

        void Handle_Clicked(object sender, EventArgs e)
        {
            if (!IsReadOnly && IsEnabled)
            {
                if (UseTextField && !TextControl.IsFocused)
                {
                    TextControl.Focus();
                }
                else if (!DateControl.IsFocused)
                {
                    DateControl.Focus();
                }
            }
        }

        void Control_Focused(object sender, FocusEventArgs e)
        {
            if (IsReadOnly || !IsEnabled)
            {
                DateControl.Unfocus();
                TextControl.Unfocus();
            }
        }

        void TextControl_Completed(object sender, EventArgs e)
        {
            var locale = _localize.GetLocale() ?? CultureInfo.CurrentUICulture;
            var dfi = locale.DateTimeFormat;

            if (DateTime.TryParse(TextControl.Text, dfi, DateTimeStyles.AllowWhiteSpaces, out DateTime result))
            {
                var dateChangedEventArgs = new DateChangedEventArgs(Date, result.Date);
                if (!Date.Date.Equals(result.Date))
                {
                    Date = result.Date;
                    DateSelected?.Invoke(this, dateChangedEventArgs);
                }
            }
            else
            {
                TextControl.Text = _resourceContainer.GetFormattedString("{0:d}", Date);
            }
        }

        void DateControl_DateSelected(object sender, EventArgs e)
        {
            var dateChangedEventArgs = new DateChangedEventArgs(Date, DateControl.Date);
            if (!Date.Date.Equals(DateControl.Date.Date))
            {
                Date = DateControl.Date.Date;
                DateSelected?.Invoke(this, dateChangedEventArgs);
            }
        }

        static void UpdateErrorAndHint(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is DatePicker datePicker && oldValue != newValue)
            {
                if (!String.IsNullOrEmpty(datePicker.Error))
                {
                    datePicker.HintErrorControl.IsVisible = true;
                    datePicker.HintErrorControl.Text = datePicker.Error;
                    if (datePicker._compact)
                    {
                        datePicker.HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlErrorLabelCompactStyle"];
                    }
                    else
                    {
                        datePicker.HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlErrorLabelCompactStyle"];
                    }
                }
                else if (!String.IsNullOrEmpty(datePicker.Hint))
                {
                    datePicker.HintErrorControl.IsVisible = true;
                    datePicker.HintErrorControl.Text = datePicker.Hint;
                    if (datePicker._compact)
                    {
                        datePicker.HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlHintLabelCompactStyle"];
                    }
                    else
                    {
                        datePicker.HintErrorControl.Style = (Xamarin.Forms.Style)DynamicResourceProvider.Instance["ControlHintLabelCompactStyle"];
                    }
                }
                else
                {
                    datePicker.HintErrorControl.IsVisible = false;
                }
            }
        }
    }
}

