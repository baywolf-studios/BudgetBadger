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

            if (e.NewElement != null && Control != null && Control is NSScrollView scroller)
            {
                if (NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(11, 0, 0)))
                {
                    NativeTableView.Style = NSTableViewStyle.Plain;
                }
                NativeTableView.IntercellSpacing = new CoreGraphics.CGSize(0, 0);
                NativeTableView.BackgroundColor = e.NewElement.BackgroundColor.ToNSColor();

                NSScrollView.Notifications.ObserveDidLiveScroll(scroller, (sender, args) => {
                    if (e.NewElement != null && args.Notification.Object is NSScrollView nsScrollView)
                    {
                        if (nsScrollView.DocumentVisibleRect != null)
                        {
                            var scrolledArgs = new ScrolledEventArgs(nsScrollView.DocumentVisibleRect.X, nsScrollView.DocumentVisibleRect.Y);
                            e.NewElement.SendScrolled(scrolledArgs);
                        }
                    }
                });
            }
        }
    }
}
