using System;
using AppKit;
using BudgetBadger.macOS.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportEffect(typeof(BorderlessPickerEffect), "BorderlessPickerEffect")]
namespace BudgetBadger.macOS.Effects
{
    public class BorderlessPickerEffect : PlatformEffect
    {
        public BorderlessPickerEffect()
        {
        }

        protected override void OnAttached()
        {
            var defaultFont = NSFont.SystemFontOfSize(12);
            var testFont = NSFont.FromFontName(".AppleSystemUIFont-Regular", 12);

            if (Control is NSPopUpButton picker)
            {
                picker.Layer.BorderWidth = 0;
                picker.Bordered = false;
                picker.FocusRingType = NSFocusRingType.None;
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
