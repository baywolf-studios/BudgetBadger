using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BudgetBadger.Forms.Extensions
{
    [AcceptEmptyServiceProvider]
    [ContentProperty(nameof(GlyphKey))]
    public class FontImageExtension : IMarkupExtension<ImageSource>
    {
        public string FontFamilyKey { get; set; }
        public string GlyphKey { get; set; }
        public string ColorKey { get; set; }
        public string SizeKey { get; set; }

        public ImageSource ProvideValue(IServiceProvider serviceProvider)
        {
            var fontImageSource = new FontImageSource();
            fontImageSource.SetDynamicResource(FontImageSource.FontFamilyProperty, FontFamilyKey);
            fontImageSource.SetDynamicResource(FontImageSource.GlyphProperty, GlyphKey);
            fontImageSource.SetDynamicResource(FontImageSource.ColorProperty, ColorKey);
            fontImageSource.SetDynamicResource(FontImageSource.SizeProperty, SizeKey);
            return fontImageSource;
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return (this as IMarkupExtension<ImageSource>).ProvideValue(serviceProvider);
        }
    }
}
