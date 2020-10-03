using System;
using AppKit;
using BudgetBadger.macOS.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

[assembly: ExportRenderer(typeof(Editor), typeof(BudgetBadger.macOS.Renderers.EditorRenderer))]
namespace BudgetBadger.macOS.Renderers
{
    public class EditorRenderer : Xamarin.Forms.Platform.MacOS.EditorRenderer
    {
        public static void Initialize()
        {
            // empty, but used for beating the linker
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null && Control is NSTextField textField)
            {
                //textField.Appearance = NSAppearance.GetAppearance(NSAppearance.NameAqua);
            }
        }
    }
}
