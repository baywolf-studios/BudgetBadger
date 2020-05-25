using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(MasterDetailPage), typeof(BudgetBadger.macOS.Renderers.MasterDetailRenderer))]
namespace BudgetBadger.macOS.Renderers
{
    public class MasterDetailRenderer : MasterDetailPageRenderer
    {
        protected override double MasterWidthPercentage
        {
            get
            {
                if (View != null && View.Frame != null && View.Frame.Width != -1)
                {
                    return 240 / View.Frame.Width;
                }

                return base.MasterWidthPercentage;
            }
        }
    }
}
