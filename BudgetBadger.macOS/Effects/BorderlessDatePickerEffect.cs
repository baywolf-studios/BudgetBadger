using System;
using AppKit;
using BudgetBadger.macOS.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportEffect(typeof(BorderlessDatePickerEffect), "BorderlessDatePickerEffect")]
namespace BudgetBadger.macOS.Effects
{
    public class BorderlessDatePickerEffect : PlatformEffect
    {
        public BorderlessDatePickerEffect()
        {
        }

        protected override void OnAttached()
        {
            if (Control is NSDatePicker datePicker)
            {
                datePicker.Layer.BorderWidth = 0;
                datePicker.BackgroundColor = NSColor.Clear;
                datePicker.Bordered = false;
                datePicker.FocusRingType = NSFocusRingType.None;
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
