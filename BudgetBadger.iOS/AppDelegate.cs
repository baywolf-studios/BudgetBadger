using System;
using System.Collections.Generic;
using System.Linq;
using BudgetBadger.Core.Authentication;
using BudgetBadger.Forms;
using Foundation;
using Prism;
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

            return base.FinishedLaunching(app, options);
        }

        public class iOSInitializer : IPlatformInitializer
        {
            public void RegisterTypes(IContainerRegistry container)
            {
                container.Register<IOAuth2Authenticator, OAuth2Authenticator>();
            }
        }
    }
}
