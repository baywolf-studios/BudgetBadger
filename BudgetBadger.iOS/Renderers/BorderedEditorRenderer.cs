using System;
using System.ComponentModel;
using BudgetBadger.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Editor), typeof(BorderedEditorRenderer))]
namespace BudgetBadger.iOS.Renderers
{
    public class BorderedEditorRenderer : EditorRenderer
    {
        public static void Initialize()
        {
            // empty, but used for beating the linker
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (Control != null && Control is UITextView textView)
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                {
                    var gray = UIColor.FromName("EditorBorderColor");
                    textView.Layer.BorderColor = gray.CGColor;
                }
                else
                {
                    var lightGray = Color.FromRgb(205, 205, 205);
                    textView.Layer.BorderColor = lightGray.ToCGColor();
                }

                textView.Layer.BorderWidth = 0.5f;
                textView.Layer.CornerRadius = 5f;
            }
        }
    }
}
