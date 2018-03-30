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
            if (Control is NSTextField entry)
            {
                entry.Layer.BorderWidth = 0;
                entry.BackgroundColor = NSColor.Clear;
                entry.Bordered = false;
                entry.FocusRingType = NSFocusRingType.None;
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
