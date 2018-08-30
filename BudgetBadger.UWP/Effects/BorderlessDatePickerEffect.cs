using System;
using BudgetBadger.UWP.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportEffect(typeof(BorderlessDatePickerEffect), "BorderlessDatePickerEffect")]
namespace BudgetBadger.UWP.Effects
{
    public class BorderlessDatePickerEffect : PlatformEffect
    {
        public BorderlessDatePickerEffect()
        {
        }

        protected override void OnAttached()
        {
            if (Control is Windows.UI.Xaml.Controls.DatePicker control)
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
