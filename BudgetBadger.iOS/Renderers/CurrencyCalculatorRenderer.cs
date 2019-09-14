using System;
using BudgetBadger.Forms.UserControls;
using BudgetBadger.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CurrencyCalculator), typeof(CurrencyCalculatorRenderer))]
namespace BudgetBadger.iOS.Renderers
{
    public class CurrencyCalculatorRenderer : EntryRenderer
    {
        public static void Initialize()
        {
            // empty, but used for beating the linker
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null && Control is UITextField control)
            {
                var toolbar = new UIToolbar
                {
                    Items = new[]
                    {
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    new UIBarButtonItem("+", UIBarButtonItemStyle.Plain, (sender, e2) => control.Text += "+"),
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    new UIBarButtonItem("-", UIBarButtonItemStyle.Plain, (sender, e2) => control.Text += "-"),
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    new UIBarButtonItem("*", UIBarButtonItemStyle.Plain, (sender, e2) => control.Text += "*"),
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    new UIBarButtonItem("/", UIBarButtonItemStyle.Plain, (sender, e2) => control.Text += "/"),
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    new UIBarButtonItem(UIBarButtonSystemItem.Done, delegate { Control.ResignFirstResponder(); }),
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
                }
                };
                toolbar.SizeToFit();

                control.KeyboardType = UIKeyboardType.DecimalPad;
                control.InputAccessoryView = toolbar;
            }
        }
    }
}
