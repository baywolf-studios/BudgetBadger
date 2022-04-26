using System;
using AppKit;
using BudgetBadger.macOS.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportEffect(typeof(BorderlessEditorEffect), "BorderlessEditorEffect")]
namespace BudgetBadger.macOS.Effects
{
    public class BorderlessEditorEffect : PlatformEffect
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
