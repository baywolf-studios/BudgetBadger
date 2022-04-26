using System;
using System.ComponentModel;
using BudgetBadger.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(DatePicker), typeof(BudgetBadger.iOS.Renderers.DatePickerRenderer))]
namespace BudgetBadger.iOS.Renderers
{
	public class DatePickerRenderer : Xamarin.Forms.Platform.iOS.DatePickerRenderer
	{
		protected override void OnElementChanged(Xamarin.Forms.Platform.iOS.ElementChangedEventArgs<DatePicker> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null && this.Control != null)
			{
				try
				{
					UIDatePicker picker = (UIDatePicker)Control.InputView;
					if (UIDevice.CurrentDevice.CheckSystemVersion(13, 4))
					{
						//select desired style
						picker.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
					}
					UpdateDateFromModel(picker, false);

				}
				catch
				{
					// do nothing
				}
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			try
			{
				try
				{
					UIDatePicker picker = (UIDatePicker)Control.InputView;
					UpdateDateFromModel(picker, true);

				}
				catch
				{
					// do nothing
				}
			}
			catch { }
		}

		private void UpdateDateFromModel(UIDatePicker picker, bool animate)
		{
			if (picker.Date.ToDateTime().Date != Element.Date.Date)
				picker.SetDate(Element.Date.ToNSDate(), animate);
			Control.Text = Element.Date.ToString(Element.Format);
		}
	}
}
