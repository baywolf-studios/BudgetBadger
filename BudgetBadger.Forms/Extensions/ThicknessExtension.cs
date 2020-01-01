using System;
using BudgetBadger.Forms.Style;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BudgetBadger.Forms.Extensions
{
    public class ThicknessExtension : IMarkupExtension<BindingBase>
    {
        public string LeftKey { get; set; }
        public string TopKey { get; set; }
        public string RightKey { get; set; }
        public string BottomKey { get; set; }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return ProvideValue(serviceProvider);
        }

        public BindingBase ProvideValue(IServiceProvider serviceProvider)
        {
            var key = string.Join(",", LeftKey, TopKey, RightKey, BottomKey);

            var binding = new Binding
            {
                Mode = BindingMode.OneWay,
                Path = $"[{key}]",
                Source = ThicknessProvider.Instance
            };
            return binding;
        }
    }
}
