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
            if (Control is NSPopUpButton picker)
            {
                picker.Cell.Bordered = false;
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
