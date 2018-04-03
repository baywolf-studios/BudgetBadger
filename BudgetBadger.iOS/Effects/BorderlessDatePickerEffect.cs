using System;
using BudgetBadger.iOS.Effects;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportEffect(typeof(BorderlessDatePickerEffect), "BorderlessDatePickerEffect")]
namespace BudgetBadger.iOS.Effects
{
    public class BorderlessDatePickerEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Control is UITextField datePicker)
            {
                //datePicker.VerticalAlignment = UIControlContentVerticalAlignment.Center;
                datePicker.Layer.BorderWidth = 0;
                datePicker.BorderStyle = UITextBorderStyle.None;
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
