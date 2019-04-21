using System;
using AppKit;
using CoreGraphics;

namespace BudgetBadger.macOS.Renderers
{
    public static class Helper
    {
        internal static void Elevate(this NSView view, float elevation)
        {
            view.Shadow = new NSShadow();
            view.Layer.MasksToBounds = false;
            view.Layer.ShadowColor = NSColor.Black.CGColor;
            view.Layer.ShadowOffset = new CGSize(0, (nfloat)(-1 * elevation));
            view.Layer.ShadowOpacity = 0.24f;
            view.Layer.ShadowRadius = Math.Abs(elevation);
        }
    }
}
