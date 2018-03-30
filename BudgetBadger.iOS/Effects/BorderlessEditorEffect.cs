using System;
using BudgetBadger.iOS.Effects;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportEffect(typeof(BorderlessEditorEffect), "BorderlessEditorEffect")]
namespace BudgetBadger.iOS.Effects
{
    public class BorderlessEditorEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Control is UITextView control)
            {
                control.Layer.BorderWidth = 0;
                control.TextContainerInset = UIEdgeInsets.Zero;
                control.ContentInset = UIEdgeInsets.Zero;
                control.TextContainer.LineFragmentPadding = 0;
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
