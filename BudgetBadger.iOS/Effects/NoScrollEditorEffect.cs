using System;
using BudgetBadger.iOS.Effects;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportEffect(typeof(NoScrollEditorEffect), "NoScrollEditorEffect")]
namespace BudgetBadger.iOS.Effects
{
    public class NoScrollEditorEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Control is UITextView control)
            {
                control.ScrollEnabled = false;
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
