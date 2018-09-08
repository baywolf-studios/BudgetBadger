using FFImageLoading.Forms.Platform;
using FFImageLoading.Svg.Forms;
using Prism;
using Prism.Ioc;
using Syncfusion.ListView.XForms.UWP;
using Syncfusion.SfDataGrid.XForms.UWP;
using Syncfusion.SfPullToRefresh.XForms.UWP;
using Syncfusion.XForms.UWP.Buttons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace BudgetBadger.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            SfDataGridRenderer.Init();
            SfListViewRenderer.Init();
            SfPullToRefreshRenderer.Init();
            SfCheckBoxRenderer.Init();
            CachedImageRenderer.Init();
            var ignore = typeof(SvgCachedImage);
            LoadApplication(new BudgetBadger.Forms.App(new UwpInitializer()));
        }
    }

    public class UwpInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry container)
        {

        }
    }
}
