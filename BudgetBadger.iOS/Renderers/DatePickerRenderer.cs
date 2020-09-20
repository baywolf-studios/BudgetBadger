using System;
using BudgetBadger.iOS.Renderers;
using UIKit;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(DatePicker), typeof(DatePickerRenderer))]
namespace BudgetBadger.iOS.Renderers
{
    public class DatePickerRenderer : Xamarin.Forms.Platform.iOS.DatePickerRenderer
	{
		protected override void OnElementChanged(Xamarin.Forms.Platform.iOS.ElementChangedEventArgs<DatePicker> e)
		{
			base.OnElementChanged(e);
			if (UIDevice.CurrentDevice.CheckSystemVersion(13, 4))
			{
				if (Control != null)
				{
					UITextField entry = Control;
					UIDatePicker picker = (UIDatePicker)entry.InputView;
					picker.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
				}
			}
		}
	}
}
