using BudgetBadger.Core.LocalizedResources;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BudgetBadger.Forms.Extensions
{
    [ContentProperty("Text")]
    public class TranslateExtension : IMarkupExtension
    {
        static readonly Lazy<IResourceContainer> _resourceContainer = new Lazy<IResourceContainer>(() => StaticResourceContainer.Current);

        public string Text { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return _resourceContainer.Value.GetResourceString(Text);
        }
    }
}
