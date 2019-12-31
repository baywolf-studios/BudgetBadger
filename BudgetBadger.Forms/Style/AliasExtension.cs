using System;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace BudgetBadger.Forms.Style
{
    [ContentProperty(nameof(Key))]
    public sealed class AliasExtension : IMarkupExtension<DynamicResource>
    {
        public string Key { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider) => ((IMarkupExtension<DynamicResource>)this).ProvideValue(serviceProvider);

        DynamicResource IMarkupExtension<DynamicResource>.ProvideValue(IServiceProvider serviceProvider)
        {
            if (Key == null)
                throw new XamlParseException("DynamicResource markup require a Key");
            return new DynamicResource(Key);
        }
    }
}
