using System;
using BudgetBadger.iOS.Effects;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportEffect(typeof(BorderlessPickerEffect), "BorderlessPickerEffect")]
namespace BudgetBadger.iOS.Effects
{
    public class BorderlessPickerEffect : PlatformEffect
    {
        public BorderlessPickerEffect()
        {
        }

        protected override void OnAttached()
        {
            if (Control is UITextField control)
            {
                control.Layer.BorderWidth = 0;
                control.BorderStyle = UITextBorderStyle.None;
            }
        }

        protected override void OnDetached()
        {
        }
    }
}

