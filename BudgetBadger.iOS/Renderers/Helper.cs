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
            view.Layer.ShadowOffset = new CGSize(0, (nfloat)elevation);
            view.Layer.ShadowOpacity = 0.24f;
            view.Layer.ShadowRadius = Math.Abs(elevation);
        }
    }
}
