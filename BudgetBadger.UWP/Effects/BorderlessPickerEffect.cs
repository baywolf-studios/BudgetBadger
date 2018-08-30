using System;
using BudgetBadger.UWP.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportEffect(typeof(BorderlessPickerEffect), "BorderlessPickerEffect")]
namespace BudgetBadger.UWP.Effects
{
    public class BorderlessPickerEffect : PlatformEffect
    {
        public BorderlessPickerEffect()
        {
        }

        protected override void OnAttached()
        {
            if (Control is FormsComboBox picker)
            {
                picker.BorderThickness = new Windows.UI.Xaml.Thickness(0);

                picker.Margin = new Windows.UI.Xaml.Thickness(0);

                picker.Padding = new Windows.UI.Xaml.Thickness(0);
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
