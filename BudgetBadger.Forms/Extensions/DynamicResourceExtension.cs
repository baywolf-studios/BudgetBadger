using System;
using BudgetBadger.Forms.Style;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BudgetBadger.Forms.Extensions
{
    [ContentProperty("Key")]
    public class DynamicResourceExtension : IMarkupExtension<BindingBase>
    {
        public string Key { get; set; }
        public IValueConverter Converter { get; set; }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValue(serviceProvider);
        }

        public BindingBase ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new Binding
            {
                Mode = BindingMode.OneWay,
                Path = $"[{Key}]",
                Source = DynamicResourceProvider.Instance,
                Converter = Converter
            };
            return binding;
        }
    }
}
