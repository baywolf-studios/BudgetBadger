using System;
using AppKit;
using BudgetBadger.macOS.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(Entry), typeof(BudgetBadger.macOS.Renderers.EntryRenderer))]
namespace BudgetBadger.macOS.Renderers
{
    public class EntryRenderer : Xamarin.Forms.Platform.MacOS.EntryRenderer
    {
        public static void Initialize()
        {
            // empty, but used for beating the linker
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null && Control is NSTextField textField)
            {
                textField.Appearance = NSAppearance.GetAppearance(NSAppearance.NameAqua);
            }
        }
    }
}
