using System;
using BudgetBadger.iOS.Effects;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportEffect(typeof(CalculatorKeyboardEntryEffect), "CalculatorKeyboardEntryEffect")]
namespace BudgetBadger.iOS.Effects
{
    public class CalculatorKeyboardEntryEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Control is UITextField control)
            {
                var toolbar = new UIToolbar
                {
                    Items = new[]
                    {
                        new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                        new UIBarButtonItem("+", UIBarButtonItemStyle.Plain, (sender, e) => control.Text += "+"),
                        new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                        new UIBarButtonItem("-", UIBarButtonItemStyle.Plain, (sender, e) => control.Text += "-"),
                        new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                        new UIBarButtonItem("*", UIBarButtonItemStyle.Plain, (sender, e) => control.Text += "*"),
                        new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                        new UIBarButtonItem("/", UIBarButtonItemStyle.Plain, (sender, e) => control.Text += "/"),
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

        protected override void OnDetached()
        {
        }
    }
}