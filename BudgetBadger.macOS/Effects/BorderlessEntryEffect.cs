using System;
using AppKit;
using BudgetBadger.macOS.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ResolutionGroupName("Wolf")]
[assembly: ExportEffect(typeof(BorderlessEntryEffect), "BorderlessEntryEffect")]
namespace BudgetBadger.macOS.Effects
{
    public class BorderlessEntryEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Control is NSTextField control)
            {
                control.Layer.BorderWidth = 0;
                control.BackgroundColor = NSColor.Clear;
                control.Bordered = false;
                control.FocusRingType = NSFocusRingType.None;
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
