using System;
using AppKit;
using CoreGraphics;
using BudgetBadger.macOS.Renderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(DatePicker), typeof(CustomDatePickerRenderer))]
namespace BudgetBadger.macOS.Renderer
{
    public class CustomDatePickerRenderer : DatePickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.DatePickerStyle = NSDatePickerStyle.TextFieldAndStepper;
            }
        }
    }
}