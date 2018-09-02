using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using BudgetBadger.Forms;
using Prism;
using Prism.Ioc;
using FFImageLoading.Forms.Droid;
using FFImageLoading.Svg.Forms;

namespace BudgetBadger.Droid
{
    [Activity(Label = "BudgetBadger.Droid", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            SimpleAuth.NativeCustomTabsAuthenticator.Activate(this.Application);
            SfPullToRefreshRenderer.Init();
            Syncfusion.SfDataGrid.XForms.Droid.SfDataGridRenderer.Init();
            Syncfusion.ListView.XForms.Droid.SfListViewRenderer.Init();
            CachedImageRenderer.Init();
            var ignore = typeof(SvgCachedImage);
            

            LoadApplication(new App(new DroidInitializer()));
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            SimpleAuth.Native.OnActivityResult(requestCode, resultCode, data);
        }
    }

    public class DroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry container)
        {
        }
    }
}
