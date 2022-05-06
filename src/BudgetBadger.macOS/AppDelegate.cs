using AppKit;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Forms;
using BudgetBadger.macOS.Renderers;
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

            var rect = new CoreGraphics.CGRect(200, 1000, 1280, 720);
            window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
            window.Title = "Budget Badger";
            window.TitleVisibility = NSWindowTitleVisibility.Hidden;
        }

        public override NSWindow MainWindow
        {
            get { return window; }
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            Xamarin.Forms.Forms.Init();
            Button2Renderer.Initialize();
            MasterDetailRenderer.Initialize();

            LoadApplication(new App(new macOSInitializer()));
            base.DidFinishLaunching(notification);
        }


        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }

        [Export("applicationShouldTerminateAfterLastWindowClosed:")]
        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
        {
            return true;
        }
    }

    public class macOSInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry container)
        {
            container.Register<ILocalize, Localize>();
            var test = new InAppBillingImplementation();
            container.RegisterInstance<IInAppBilling>(test);
        }
    }
}
