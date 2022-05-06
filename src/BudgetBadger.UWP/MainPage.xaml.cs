using BudgetBadger.Core.LocalizedResources;
using BudgetBadger.Forms;
using BudgetBadger.Core.Authentication;
using BudgetBadger.UWP.Renderers;
using DryIoc;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;
using Prism;
using Prism.Ioc;
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
            Button2Renderer.Initialize();
            ContentButtonRenderer.Initialize();
            LoadApplication(new BudgetBadger.Forms.App(new UwpInitializer()));
        }
    }

    public class UwpInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry container)
        {
            container.Register<ILocalize, Localize>();
            container.Register<IWebAuthenticator, UwpWebAuthenticator>();
        }
    }
}
