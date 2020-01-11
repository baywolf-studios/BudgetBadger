using System;
using AppKit;
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
                NSScrollView.Notifications.ObserveDidLiveScroll(scroller, (sender, args) => {
                    if (args.Notification.Object is NSScrollView nsScrollView)
                    {
                        var scrolledArgs = new ScrolledEventArgs(nsScrollView.DocumentVisibleRect.X, nsScrollView.DocumentVisibleRect.Y);
                        Element.SendScrolled(scrolledArgs);
                    }
                });
            }
        }
    }
}
