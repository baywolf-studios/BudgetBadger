using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Forms;
using DryIoc;
using Foundation;
using Prism;
using Prism.DryIoc;
using Prism.Ioc;
using UIKit;

namespace BudgetBadger.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            LoadApplication(new App(new iOSInitializer()));

            SimpleAuth.NativeSafariAuthenticator.Activate();

            UITabBar.Appearance.SelectedImageTintColor = UIColor.FromRGB(54, 120, 175);

            return base.FinishedLaunching(app, options);
        }

        public class iOSInitializer : IPlatformInitializer
        {
            public void RegisterTypes(IContainerRegistry container)
            {
            }
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
}
