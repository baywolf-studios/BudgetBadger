using BudgetBadger.Forms;
using FFImageLoading.Forms.Platform;
using FFImageLoading.Svg.Forms;
using Foundation;
using Prism;
using Prism.Ioc;
using UIKit;


namespace BudgetBadger.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            SimpleAuth.NativeSafariAuthenticator.Activate();
            Syncfusion.SfDataGrid.XForms.iOS.SfDataGridRenderer.Init();
            Syncfusion.ListView.XForms.iOS.SfListViewRenderer.Init();
            Syncfusion.SfPullToRefresh.XForms.iOS.SfPullToRefreshRenderer.Init();
            Syncfusion.XForms.iOS.Buttons.SfCheckBoxRenderer.Init();
            CachedImageRenderer.Init();
            var ignore = typeof(SvgCachedImage);

            LoadApplication(new App(new iOSInitializer()));


            UITabBar.Appearance.SelectedImageTintColor = UIColor.FromRGB(54, 120, 175);

            var statusBar = UIApplication.SharedApplication.ValueForKey(new NSString("statusBar")) as UIView;
            if (statusBar.RespondsToSelector(new ObjCRuntime.Selector("setBackgroundColor:")))
            {
                statusBar.BackgroundColor = UIColor.FromRGB(54, 120, 175);
            }

            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            if (SimpleAuth.NativeSafariAuthenticator.ResumeAuth(url.AbsoluteString))
            {
                return true;
            }
            if (SimpleAuth.Native.OpenUrl(app, url, options))
            {
                return true;
            }
            return base.OpenUrl(app, url, options);
        }
    }

    public class iOSInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry container)
        {

        }
    }
}
