using System;
using AppKit;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(ListView), typeof(BudgetBadger.macOS.Renderers.ListViewRenderer))]
namespace BudgetBadger.macOS.Renderers
{
    public class ListViewRenderer : Xamarin.Forms.Platform.MacOS.ListViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null || Control == null || !(Control is NSScrollView scroller))
            {
                return;
            }

            if (NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(11, 0, 0)))
            {
                NativeTableView.Style = NSTableViewStyle.Plain;
            }
            NativeTableView.IntercellSpacing = new CoreGraphics.CGSize(0, 0);
            NativeTableView.BackgroundColor = e.NewElement.BackgroundColor.ToNSColor();
        }
    }
}
