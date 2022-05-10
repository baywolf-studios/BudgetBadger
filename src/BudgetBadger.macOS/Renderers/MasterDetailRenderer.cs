using System;
using Xamarin.Forms;
using BudgetBadger.Forms.UserControls;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(MasterDetailPage), typeof(BudgetBadger.macOS.Renderers.MasterDetailRenderer))]
namespace BudgetBadger.macOS.Renderers
{
    public class MasterDetailRenderer : MasterDetailPageRenderer
    {
        public static void Initialize()
        {
            // empty, but used for beating the linker
        }

        protected override double MasterWidthPercentage
        {
            get
            {
                if (View != null && View.Frame != null && View.Frame.Width != -1)
                {
                    return 300 / View.Frame.Width;
                }

                return base.MasterWidthPercentage;
            }
        }
    }
}
