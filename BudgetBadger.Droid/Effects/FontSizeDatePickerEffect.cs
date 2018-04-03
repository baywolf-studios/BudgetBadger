using System;
using System.Linq;
using Android.Widget;
using BudgetBadger.Droid.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportEffect(typeof(FontSizeDatePickerEffect), "FontSizeDatePickerEffect")]
namespace BudgetBadger.Droid.Effects
{
    public class FontSizeDatePickerEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            var fontSizeParam = (Forms.Effects.FontSizeDatePickerEffect)Element.Effects.FirstOrDefault(e => e is Forms.Effects.FontSizeDatePickerEffect);

            if (Control is EditText datePicker && fontSizeParam != null)
            {
                datePicker.TextSize = (float)fontSizeParam.FontSize;
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
