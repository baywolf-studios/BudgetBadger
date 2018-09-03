using System;
using BudgetBadger.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Editor), typeof(CustomEditorRenderer))]
namespace BudgetBadger.iOS.Renderers
{
    public class CustomEditorRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null && Control is UITextView textView)
            {
                var borderColor = UIColor.FromRGBA((byte)0.8, (byte)0.8, (byte)0.8, (byte)1.0);

                textView.Layer.BorderColor = borderColor.CGColor;
                textView.Layer.BorderWidth = 1f;
                textView.Layer.CornerRadius = 5f;
            }
        }
    }
}
