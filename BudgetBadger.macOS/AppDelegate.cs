using AppKit;
using BudgetBadger.Forms;
using FFImageLoading.Forms.Platform;
using FFImageLoading.Svg.Forms;
using Foundation;
using Plugin.InAppBilling.Abstractions;
using Prism;
using Prism.Ioc;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace BudgetBadger.macOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        NSWindow window;

        public AppDelegate()
        {
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

            var rect = new CoreGraphics.CGRect(200, 1000, 1024, 768);
            window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
            window.Title = "Xamarin.Forms on Mac!"; // choose your own Title here
            window.TitleVisibility = NSWindowTitleVisibility.Hidden;
        }

        public override NSWindow MainWindow
        {
            get { return window; }
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            Xamarin.Forms.Forms.Init();
            SimpleAuth.NativeSafariAuthenticator.Activate();
            Syncfusion.SfDataGrid.XForms.MacOS.SfDataGridRenderer.Init();
            Syncfusion.ListView.XForms.MacOS.SfListViewRenderer.Init();
            CachedImageRenderer.Init();
            var ignore = typeof(SvgCachedImage);

            LoadApplication(new App(new macOSInitializer()));
            base.DidFinishLaunching(notification);
        }


        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }

    public class macOSInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry container)
        {
        }
    }
}
