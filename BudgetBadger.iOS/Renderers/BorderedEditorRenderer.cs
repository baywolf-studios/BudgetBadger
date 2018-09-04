using System;
using BudgetBadger.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Editor), typeof(BorderedEditorRenderer))]
namespace BudgetBadger.iOS.Renderers
{
    public class BorderedEditorRenderer : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null && Control is UITextView textView)
            {
                var lightGray = Color.FromRgb(205, 205, 205);

                textView.Layer.BorderColor = lightGray.ToCGColor();
                textView.Layer.BorderWidth = 0.5f;
                textView.Layer.CornerRadius = 5f;
            }
        }
    }
}
