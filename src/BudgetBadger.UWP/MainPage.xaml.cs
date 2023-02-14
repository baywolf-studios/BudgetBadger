using BudgetBadger.Core.Authentication;
using BudgetBadger.Core.Localization;
using BudgetBadger.UWP.Renderers;
using DryIoc;
using Prism;
using Prism.Ioc;

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
