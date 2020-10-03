using System;
using AppKit;
using BudgetBadger.macOS.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(DatePicker), typeof(BudgetBadger.macOS.Renderers.DatePickerRenderer))]
namespace BudgetBadger.macOS.Renderers
{
    public class DatePickerRenderer : Xamarin.Forms.Platform.MacOS.DatePickerRenderer
    {
        public static void Initialize()
        {
            // empty, but used for beating the linker
        }

        protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
        {
            base.OnElementChanged(e);

            if (Control != null && Control is NSDatePicker datePicker)
            {
                //datePicker.Appearance = NSAppearance.GetAppearance(NSAppearance.NameAqua);
            }
        }
    }
}
