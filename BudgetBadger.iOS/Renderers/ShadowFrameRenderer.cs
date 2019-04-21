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

            if (e.NewElement == null)
                return;
            UpdateShadow();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == "Elevation" || e.PropertyName == "HasShadow")
            {
                UpdateShadow();
            }
        }

        private void UpdateShadow()
        {

            var materialFrame = (ShadowFrame)Element;

            // Update shadow to match better material design standards of elevation
            Layer.ShadowRadius = materialFrame.Elevation;
            Layer.ShadowColor = UIColor.Gray.CGColor;
            Layer.ShadowOffset = new CGSize(2, 2);
            Layer.ShadowOpacity = 0.80f;
            Layer.ShadowPath = UIBezierPath.FromRect(Layer.Bounds).CGPath;
            Layer.MasksToBounds = false;

        }
    }
}
