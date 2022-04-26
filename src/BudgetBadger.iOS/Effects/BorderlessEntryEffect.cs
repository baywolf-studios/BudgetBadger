using System;
using BudgetBadger.iOS.Effects;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;

[assembly: ResolutionGroupName("Wolf")]
[assembly: ExportEffect(typeof(BorderlessEntryEffect), "BorderlessEntryEffect")]
namespace BudgetBadger.iOS.Effects
{
    public class BorderlessEntryEffect : PlatformEffect
    {
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
