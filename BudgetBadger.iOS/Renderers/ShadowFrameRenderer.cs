using System;
using System.ComponentModel;
using BudgetBadger.Forms.UserControls;
using BudgetBadger.iOS.Renderers;
using CoreGraphics;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ShadowFrame), typeof(ShadowFrameRenderer))]
namespace BudgetBadger.iOS.Renderers
{
    public class ShadowFrameRenderer : FrameRenderer 
    {
        public static void Initialize()
        {
            // empty, but used for beating the linker
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        {
            base.OnElementChanged(e);

            if (e?.NewElement == null) return;
            if ((Element as ShadowFrame) != null)
            {
                Element.IsClippedToBounds = true;
                UpdateShadow();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e?.PropertyName == "Elevation" || e?.PropertyName == nameof(VisualElement.BackgroundColor))
            {
                UpdateShadow();
            }
        }

        private void UpdateShadow()
        {
            var materialFrame = (ShadowFrame)Element;

            Layer.MasksToBounds = false;
            Layer.ShadowColor = UIColor.Black.CGColor;
            Layer.ShadowOffset = new CGSize(0, (nfloat)materialFrame.Elevation);
            Layer.ShadowOpacity = 0.24f;
            Layer.ShadowRadius = Math.Abs(materialFrame.Elevation);

        }
    }
}
