using System;
using BudgetBadger.UWP.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ResolutionGroupName("Wolf")]
[assembly: ExportEffect(typeof(BorderlessEntryEffect), "BorderlessEntryEffect")]
namespace BudgetBadger.UWP.Effects
{
    public class BorderlessEntryEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Control is FormsTextBox control)
            {
                control.Style = Windows.UI.Xaml.Application.Current.Resources["FormsTextBoxStyle"] as Windows.UI.Xaml.Style;

                var margin = control.BorderThickness;
                control.BorderThickness = new Windows.UI.Xaml.Thickness(0);
                control.Margin = margin;

                var padding = control.Padding;
                padding.Left = 0;
                padding.Right = 0;
                control.Padding = padding;
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
