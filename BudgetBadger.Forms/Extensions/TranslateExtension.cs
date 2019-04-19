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
            if (Text == null)
                return "";

            var translation = _resourceContainer.Value.GetString(Text);

            if (translation == null)
            {

#if DEBUG
                throw new ArgumentException(
                    String.Format("Key '{0}' was not found in resources.", Text),
                    "Text");
#else
				translation = Text; // returns the key, which GETS DISPLAYED TO THE USER
#endif
            }
            return translation;
        }
    }
}
