using System;
using BudgetBadger.Forms.Pages;
using BudgetBadger.iOS.Effects;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportEffect(typeof(StatusBarPaddingEffect), "StatusBarPaddingEffect")]
namespace BudgetBadger.iOS.Effects
{
    public class StatusBarPaddingEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Element is BasePage page)
            {
                page.SizeChanged += Page_SizeChanged;
            }
        }

        void Page_SizeChanged(object sender, EventArgs e)
        {
            var page = (BasePage)sender;

            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                var insets = UIApplication.SharedApplication.Windows[0].SafeAreaInsets; // Can't use KeyWindow this early
                page.HeaderContentView.Padding = new Thickness(insets.Left, insets.Top, insets.Right, 0);
                page.BodyContentView.Padding = new Thickness(insets.Left, 0, insets.Right, 0);
                
            }
            else
            {
                var statusHeight = UIApplication.SharedApplication.StatusBarFrame.Height;
                page.HeaderContentView.Padding = new Thickness(0, statusHeight, 0, 0);
            }
        }



        protected override void OnDetached()
        {
            if (Element is BasePage page)
            {
                page.SizeChanged -= Page_SizeChanged;
            }
        }
    }
}