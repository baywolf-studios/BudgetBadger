using System;
using System.Linq;
using BudgetBadger.iOS.Effects;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportEffect(typeof(FontSizeDatePickerEffect), "FontSizeDatePickerEffect")]
namespace BudgetBadger.iOS.Effects
{
    public class FontSizeDatePickerEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            var fontSizeParam = (Forms.Effects.FontSizeDatePickerEffect)Element.Effects.FirstOrDefault(e => e is Forms.Effects.FontSizeDatePickerEffect);

            if (Control is UITextField datePicker && fontSizeParam != null)
            {
                var font = datePicker.Font.WithSize(new nfloat(fontSizeParam.FontSize));
                datePicker.Font = font;
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
