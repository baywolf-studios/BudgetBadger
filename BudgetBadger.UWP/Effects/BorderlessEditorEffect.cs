using System;
using BudgetBadger.UWP.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportEffect(typeof(BorderlessEditorEffect), "BorderlessEditorEffect")]
namespace BudgetBadger.UWP.Effects
{
    public class BorderlessEditorEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Control is FormsTextBox control)
            {
                control.BorderThickness = new Windows.UI.Xaml.Thickness(0);

                control.Margin = new Windows.UI.Xaml.Thickness(0);

                control.Padding = new Windows.UI.Xaml.Thickness(0);
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
