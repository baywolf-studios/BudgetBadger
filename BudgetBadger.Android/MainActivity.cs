using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Droid;
using BudgetBadger.Droid.Renderers;
using BudgetBadger.Forms;
using FFImageLoading.Forms.Platform;
using FFImageLoading.Svg.Forms;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;
using Prism;
using Prism.Ioc;

namespace BudgetBadger.Droid
{
    [Activity(Label = "Budget Badger", Icon = "@mipmap/ic_launcher", RoundIcon = "@mipmap/ic_launcher_round", Theme = "@style/splashscreen", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.Window.RequestFeature(WindowFeatures.ActionBar);
            base.SetTheme(Resource.Style.MainTheme);

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, bundle);

            Xamarin.Forms.Forms.Init(this, bundle);
            SimpleAuth.NativeCustomTabsAuthenticator.Activate(this.Application);
            CachedImageRenderer.Init(true);
            var ignore = typeof(SvgCachedImage);
            Button2Renderer.Initialize();
            CardRenderer.Initialize();
            LoadApplication(new App(new AndroidInitializer()));
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            InAppBillingImplementation.HandleActivityResult(requestCode, resultCode, data);
            SimpleAuth.Native.OnActivityResult(requestCode, resultCode, data);
        }
    }

    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry container)
        {
            container.Register<ILocalize, Localize>();
        }
    }
}

