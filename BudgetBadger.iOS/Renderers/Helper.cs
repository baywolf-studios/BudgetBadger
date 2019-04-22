using System;
using CoreGraphics;
using UIKit;

namespace BudgetBadger.iOS.Renderers
{
    public static class Helper
    {
        internal static void Elevate(this UIView view, float elevation)
        {
            view.Layer.MasksToBounds = false;
            view.Layer.ShadowColor = UIColor.Black.CGColor;

            var offset = elevation < 10 ? Math.Floor(elevation / 2) + 1 : elevation - 4;
            view.Layer.ShadowOffset = new CGSize(0, (nfloat)(offset));

            var shadowOpacity = (float)(24 - Math.Round(elevation / 10)) / 100;
            view.Layer.ShadowOpacity = shadowOpacity;

            var blurRadius = elevation == 1 ? 3 : elevation * 2;
            view.Layer.ShadowRadius = Math.Abs(blurRadius);
        }
    }
}
