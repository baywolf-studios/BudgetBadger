using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using BudgetBadger.Droid;
using BudgetBadger.Forms;
using FFImageLoading.Forms.Platform;
using FFImageLoading.Svg.Forms;
using Prism;
using Prism.Ioc;

namespace BudgetBadger.Droid
{
    [Activity(Label = "Budget Badger", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            SimpleAuth.NativeCustomTabsAuthenticator.Activate(this.Application);
            CachedImageRenderer.Init(true);
            var ignore = typeof(SvgCachedImage);
            LoadApplication(new App(new AndroidInitializer()));
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            SimpleAuth.Native.OnActivityResult(requestCode, resultCode, data);
        }
    }

    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry container)
        {
            // Register any platform specific implementations
        }
    }
}

